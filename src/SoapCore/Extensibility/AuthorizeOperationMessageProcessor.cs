using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Claims;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SoapCore;
using SoapCore.ServiceModel;

namespace SoapCore.Extensibility
{
	public class AuthorizeOperationMessageProcessor : ISoapMessageProcessor
	{
		/// <summary>
		/// A dictionary that has the path of the endpoint as Key and the corresponding type as the Value. Similar to using the UseSoapEndpoint extension function.
		/// </summary>
		private readonly Dictionary<string, Type> _pathTypes;

		/// <summary>
		/// Whether to generate the soapAction without the contract name. This is also specified in the SoapOptions.
		/// </summary>
		private readonly bool _generateSoapActionWithoutContractName;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthorizeOperationMessageProcessor"/> class.
		/// </summary>
		/// <param name="pathAndTypes">A dictionary that has the path of the endpoint as Key and the corresponding type as Value. Similar to using the UseSoapEndpoint extension function.</param>
		/// <param name="generateSoapActionWithoutContractName">Whether to generate the soapAction without the contract name. Default is false.</param>
		public AuthorizeOperationMessageProcessor(Dictionary<string, Type> pathAndTypes, bool generateSoapActionWithoutContractName = false)
		{
			_pathTypes = pathAndTypes;
			_generateSoapActionWithoutContractName = generateSoapActionWithoutContractName;
		}

		public async Task<Message> ProcessMessage(Message requestMessage, HttpContext httpContext, Func<Message, Task<Message>> next)
		{
			var soapAction = HeadersHelper.GetSoapAction(httpContext, ref requestMessage);

			if (string.IsNullOrEmpty(soapAction))
			{
				throw new ArgumentException("Unable to handle request without a valid action parameter. Please supply a valid soap action.");
			}

			if (!_pathTypes.TryGetValue(httpContext.Request.Path.Value.ToLowerInvariant(), out Type serviceType) || !TryGetOperation(soapAction, serviceType, _generateSoapActionWithoutContractName, out var operation))
			{
				throw new InvalidOperationException($"No operation found for specified action: {soapAction.ToString()}");
			}

			MethodInfo methodInfo = operation!.DispatchMethod;

			bool authorized = false;

			// Get all AuthorizeAttribute attributes that affect this method
			if (methodInfo.GetCustomAttributes<AllowAnonymousAttribute>(true).Any())
			{
				authorized = true;
			}

			List<AuthorizeAttribute> authAttrs = null;
			if (!authorized)
			{
				authAttrs = methodInfo.GetCustomAttributes<AuthorizeAttribute>(true).ToList();
				Type type = methodInfo.DeclaringType;
				while (type != null && !type.Equals(typeof(ControllerBase)))
				{
					if (type.GetCustomAttributes<AllowAnonymousAttribute>().Any())
					{
						authAttrs.Clear();
						break;
					}

					authAttrs.AddRange(type.GetCustomAttributes<AuthorizeAttribute>());

					type = type.BaseType;
				}

				authorized = authAttrs.Count == 0;
			}

			var requirements = new List<IAuthorizationRequirement>();
			if (!authorized)
			{
				// Get conditions
				if (authAttrs.Count > 0)
				{
					var authResult = await httpContext.AuthenticateAsync();
					if (!authResult.Succeeded)
					{
						throw new AuthenticationException(authResult.Failure?.Message ?? "You are not Authorized.");
					}
				}

				var authorizationPolicyProvider = httpContext.RequestServices.GetService<IAuthorizationPolicyProvider>();
				foreach (var policyAndRole in authAttrs)
				{
					// Check roles
					if (!string.IsNullOrEmpty(policyAndRole.Roles))
					{
						requirements.Add(new ClaimsAuthorizationRequirement(ClaimTypes.Role, policyAndRole.Roles.Split(',')));
					}

					if (string.IsNullOrEmpty(policyAndRole.Policy))
					{
						continue;
					}

					var policy = await authorizationPolicyProvider.GetPolicyAsync(policyAndRole.Policy);

					requirements.AddRange(policy.Requirements);
				}

				authorized = requirements.Count == 0;
			}

			if (!authorized)
			{
				// Check policies
				var contextFactory = httpContext.RequestServices.GetService<IAuthorizationHandlerContextFactory>();
				var handlers = httpContext.RequestServices.GetServices<IAuthorizationHandler>();

				// Unclear what I should send in place of AuthorizationHandlerContext.Resource. (Also see: https://stackoverflow.com/a/60417327 and its comments)
				// TODO: Supply the correct resource, if the need ever arrives.
				var authContext = contextFactory.CreateContext(requirements, httpContext.User, httpContext);
				foreach (var handler in handlers)
				{
					await handler.HandleAsync(authContext);
				}

				authorized = authContext.HasSucceeded;
			}

			if (authorized)
			{
				return await next.Invoke(requestMessage);
			}
			else
			{
				throw new UnauthorizedAccessException("You are not authorized to access this resource.");
			}
		}

		private static bool TryGetOperation(string methodNameStr, Type serviceType, bool generateSoapActionWithoutContractName, out OperationDescription operation)
		{
			ReadOnlySpan<char> methodName = methodNameStr.AsSpan();

			var service = new ServiceDescription(serviceType, generateSoapActionWithoutContractName);
			operation = null;
			foreach (var o in service.Operations)
			{
				if (o.SoapAction.AsSpan().Equals(methodName, StringComparison.OrdinalIgnoreCase)
							|| o.Name.AsSpan().Equals(HeadersHelper.GetTrimmedSoapAction(methodName), StringComparison.OrdinalIgnoreCase)
							|| methodName.Equals(HeadersHelper.GetTrimmedSoapAction(o.Name.AsSpan()), StringComparison.OrdinalIgnoreCase))
				{
					operation = o;
					break;
				}
			}

			if (operation == null)
			{
				foreach (var o in service.Operations)
				{
					if (methodName.Equals(HeadersHelper.GetTrimmedClearedSoapAction(o.SoapAction.AsSpan()), StringComparison.OrdinalIgnoreCase)
							|| methodName.IndexOf(HeadersHelper.GetTrimmedSoapAction(o.Name.AsSpan()), StringComparison.OrdinalIgnoreCase) >= 0)
					{
						operation = o;
						break;
					}
				}
			}

			return operation != null;
		}
	}
}
