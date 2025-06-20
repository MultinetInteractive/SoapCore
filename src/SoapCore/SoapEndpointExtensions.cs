using System;
using System.Linq;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoapCore.Extensibility;
using SoapCore.Meta;
using SoapCore.Serializer;

#if NETCOREAPP3_0_OR_GREATER
using Microsoft.AspNetCore.Routing;
#endif

namespace SoapCore
{
	public static class SoapEndpointExtensions
	{
		public static IApplicationBuilder UseSoapEndpoint<T>(this IApplicationBuilder builder, string path, SoapEncoderOptions encoder, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return builder.UseSoapEndpoint<CustomMessage>(typeof(T), path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IApplicationBuilder UseSoapEndpoint<T, T_MESSAGE>(this IApplicationBuilder builder, string path, SoapEncoderOptions encoder, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return builder.UseSoapEndpoint<T_MESSAGE>(typeof(T), path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IApplicationBuilder UseSoapEndpoint(this IApplicationBuilder builder, Type type, string path, SoapEncoderOptions encoder, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return builder.UseSoapEndpoint<CustomMessage>(type, path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IApplicationBuilder UseSoapEndpoint<T_MESSAGE>(this IApplicationBuilder builder, Type type, string path, SoapEncoderOptions encoder, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return builder.UseSoapEndpoint<T_MESSAGE>(type, options =>
			{
				options.Path = path;
				options.EncoderOptions = SoapEncoderOptions.ToArray(encoder);
				options.SoapSerializer = serializer;
				options.CaseInsensitivePath = caseInsensitivePath;
				options.SoapModelBounder = soapModelBounder;
				options.IndentXml = indentXml;
				options.OmitXmlDeclaration = omitXmlDeclaration;
				options.WsdlFileOptions = wsdlFileOptions;
				options.SchemeOverride = schemeOverride;
			});
		}

		public static IApplicationBuilder UseSoapEndpoint<T>(this IApplicationBuilder builder, string path, SoapEncoderOptions[] encoders, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return builder.UseSoapEndpoint<T, CustomMessage>(path, encoders, serializer, caseInsensitivePath, soapModelBounder, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IApplicationBuilder UseSoapEndpoint<T, T_MESSAGE>(this IApplicationBuilder builder, string path, SoapEncoderOptions[] encoders, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return builder.UseSoapEndpoint<T_MESSAGE>(typeof(T), options =>
			{
				options.Path = path;
				options.EncoderOptions = encoders;
				options.SoapSerializer = serializer;
				options.CaseInsensitivePath = caseInsensitivePath;
				options.SoapModelBounder = soapModelBounder;
				options.IndentXml = indentXml;
				options.OmitXmlDeclaration = omitXmlDeclaration;
				options.SchemeOverride = schemeOverride;
			});
		}

		public static IApplicationBuilder UseSoapEndpoint(this IApplicationBuilder builder, Type serviceType, Action<SoapCoreOptions> options)
		{
			return builder.UseSoapEndpoint<CustomMessage>(serviceType, options);
		}

		public static IApplicationBuilder UseSoapEndpoint<T>(this IApplicationBuilder builder, Action<SoapCoreOptions> options)
		{
			return builder.UseSoapEndpoint<T, CustomMessage>(options);
		}

		public static IApplicationBuilder UseSoapEndpoint<T, T_MESSAGE>(this IApplicationBuilder builder, Action<SoapCoreOptions> options)
			where T_MESSAGE : CustomMessage, new()
		{
			return UseSoapEndpoint<T_MESSAGE>(builder, typeof(T), options);
		}

		public static IApplicationBuilder UseSoapEndpoint<T_MESSAGE>(this IApplicationBuilder builder, Type serviceType, Action<SoapCoreOptions> options)
			where T_MESSAGE : CustomMessage, new()
		{
			var opt = new SoapCoreOptions() { Path = string.Empty };
			options(opt);

			var soapOptions = SoapOptions.FromSoapCoreOptions(opt, serviceType);

			return builder.UseMiddleware<SoapEndpointMiddleware<T_MESSAGE>>(soapOptions);
		}

#if NETCOREAPP3_0_OR_GREATER
		public static IEndpointConventionBuilder UseSoapEndpoint<T>(this IEndpointRouteBuilder routes, string path, SoapEncoderOptions encoder, SoapSerializer serializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return routes.UseSoapEndpoint<T, CustomMessage>(path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T, T_MESSAGE>(this IEndpointRouteBuilder routes, string path, SoapEncoderOptions encoder, SoapSerializer serializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return routes.UseSoapEndpoint<T_MESSAGE>(typeof(T), path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint(this IEndpointRouteBuilder routes, Type type, string path, SoapEncoderOptions encoder, SoapSerializer serializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return routes.UseSoapEndpoint<CustomMessage>(type, path, encoder, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T_MESSAGE>(this IEndpointRouteBuilder routes, Type type, string path, SoapEncoderOptions encoder, SoapSerializer serializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return routes.UseSoapEndpoint<T_MESSAGE>(type, options =>
			{
				options.Path = path;
				options.EncoderOptions = SoapEncoderOptions.ToArray(encoder);
				options.SoapSerializer = serializer;
				options.CaseInsensitivePath = caseInsensitivePath;
				options.SoapModelBounder = soapModelBounder;
				options.WsdlFileOptions = wsdlFileOptions;
				options.IndentXml = indentXml;
				options.OmitXmlDeclaration = omitXmlDeclaration;
				options.SchemeOverride = schemeOverride;
			});
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T>(this IEndpointRouteBuilder routes, string path, SoapEncoderOptions[] encoders, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		{
			return routes.UseSoapEndpoint<T, CustomMessage>(path, encoders, serializer, caseInsensitivePath, soapModelBounder, wsdlFileOptions, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T, T_MESSAGE>(this IEndpointRouteBuilder routes, string path, SoapEncoderOptions[] encoders, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
			where T_MESSAGE : CustomMessage, new()
		{
			return routes.UseSoapEndpoint<T, T_MESSAGE>(path, encoders, serializer, caseInsensitivePath, soapModelBounder, null, indentXml, omitXmlDeclaration, schemeOverride);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T, T_MESSAGE>(this IEndpointRouteBuilder routes, string path, SoapEncoderOptions[] encoders, SoapSerializer serializer = SoapSerializer.DataContractSerializer, bool caseInsensitivePath = false, ISoapModelBounder soapModelBounder = null, WsdlFileOptions wsdlFileOptions = null, bool indentXml = true, bool omitXmlDeclaration = true, string schemeOverride = null)
		where T_MESSAGE : CustomMessage, new()
		{
			return routes.UseSoapEndpoint<T, T_MESSAGE>(opt =>
			{
				opt.Path = path;
				opt.EncoderOptions = encoders;
				opt.SoapSerializer = serializer;
				opt.CaseInsensitivePath = caseInsensitivePath;
				opt.SoapModelBounder = soapModelBounder;
				opt.WsdlFileOptions = wsdlFileOptions;
				opt.IndentXml = indentXml;
				opt.OmitXmlDeclaration = omitXmlDeclaration;
				opt.SchemeOverride = schemeOverride;
			});
		}

		public static IEndpointConventionBuilder UseSoapEndpoint(this IEndpointRouteBuilder routes, Type serviceType, Action<SoapCoreOptions> options)
		{
			return routes.UseSoapEndpoint<CustomMessage>(serviceType, options);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T, T_MESSAGE>(this IEndpointRouteBuilder routes, Action<SoapCoreOptions> options)
			where T_MESSAGE : CustomMessage, new()
		{
			return routes.UseSoapEndpoint<T_MESSAGE>(typeof(T), options);
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T_MESSAGE>(this IEndpointRouteBuilder routes, Type serviceType, Action<SoapCoreOptions> options)
			where T_MESSAGE : CustomMessage, new()
		{
			var opt = new SoapCoreOptions() { Path = string.Empty };
			options(opt);

			var soapOptions = SoapOptions.FromSoapCoreOptions(opt, serviceType);

			var pipeline = routes
				.CreateApplicationBuilder()
				.UseMiddleware<SoapEndpointMiddleware<T_MESSAGE>>(soapOptions)
				.Build();

			routes.Map(soapOptions.Path?.TrimEnd('/') + "/$metadata", pipeline);
			routes.Map(soapOptions.Path?.TrimEnd('/') + "/mex", pipeline);
			routes.Map(soapOptions.Path?.TrimEnd('/') + "/{methodName}", pipeline);

			return routes.Map(soapOptions.Path, pipeline)
				.WithDisplayName("SoapCore");
		}

		public static IEndpointConventionBuilder UseSoapEndpoint<T>(this IEndpointRouteBuilder routes, Action<SoapCoreOptions> options)
		{
			return UseSoapEndpoint<T, CustomMessage>(routes, options);
		}
#endif

		public static IServiceCollection AddSoapCore(this IServiceCollection serviceCollection)
		{
			return AddSoapCore<CustomMessage>(serviceCollection);
		}

		public static IServiceCollection AddSoapCore<T_MESSAGE>(this IServiceCollection serviceCollection)
			where T_MESSAGE : CustomMessage, new()
		{
			serviceCollection.TryAddSingleton<IOperationInvoker, DefaultOperationInvoker>();
			serviceCollection.TryAddSingleton<IFaultExceptionTransformer, DefaultFaultExceptionTransformer<T_MESSAGE>>();

			serviceCollection.AddSingleton<IXmlSerializationHandlerResolver>(serviceProvider => serviceType =>
			{
				var serrailizes = serviceProvider.GetServices<IXmlSerializationHandler>();
				var serializer = serrailizes.FirstOrDefault(x => x.GetType() == serviceType);
				return serializer;
			});

			return serviceCollection;
		}

		public static IServiceCollection AddSoapExceptionTransformer(this IServiceCollection serviceCollection, Func<Exception, string> transformer)
		{
			serviceCollection.TryAddSingleton(new ExceptionTransformer(transformer));
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageInspector<TService>(this IServiceCollection serviceCollection)
			where TService : class, IMessageInspector2
		{
			serviceCollection.AddScoped<IMessageInspector2, TService>();
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageInspector(this IServiceCollection serviceCollection, IMessageInspector2 messageInspector)
		{
			serviceCollection.AddSingleton(messageInspector);
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageFilter(this IServiceCollection serviceCollection, IAsyncMessageFilter messageFilter)
		{
			serviceCollection.AddSingleton(messageFilter);
			return serviceCollection;
		}

		public static IServiceCollection AddSoapWsSecurityFilter(this IServiceCollection serviceCollection, string username, string password)
		{
			return serviceCollection.AddSoapMessageFilter(new WsMessageFilter(username, password));
		}

		public static IServiceCollection AddSoapModelBindingFilter(this IServiceCollection serviceCollection, IModelBindingFilter modelBindingFilter)
		{
			serviceCollection.AddSingleton(modelBindingFilter);
			return serviceCollection;
		}

		public static IServiceCollection AddSoapServiceOperationTuner<TService>(this IServiceCollection serviceCollection)
			where TService : class, IServiceOperationTuner
		{
			serviceCollection.AddScoped<IServiceOperationTuner, TService>();
			return serviceCollection;
		}

		public static IServiceCollection AddSoapServiceOperationTuner(this IServiceCollection serviceCollection, IServiceOperationTuner serviceOperationTuner)
		{
			serviceCollection.AddSingleton(serviceOperationTuner);
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageProcessor(this IServiceCollection serviceCollection, ISoapMessageProcessor messageProcessor)
		{
			serviceCollection.AddSingleton(messageProcessor);
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageProcessor(this IServiceCollection serviceCollection, Func<Message, HttpContext, Func<Message, Task<Message>>, Task<Message>> messageProcessor)
		{
			serviceCollection.AddSingleton<ISoapMessageProcessor>(new LambdaSoapMessageProcessor(messageProcessor));
			return serviceCollection;
		}

		public static IServiceCollection AddSoapMessageProcessor<TProcessor>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProcessor : class, ISoapMessageProcessor
		{
			serviceCollection.Add(new ServiceDescriptor(typeof(ISoapMessageProcessor), typeof(TProcessor), lifetime));
			return serviceCollection;
		}

		public static IServiceCollection AddCustomSoapMessageSerializer(this IServiceCollection serviceCollection, IXmlSerializationHandler messageSerializer)
		{
			serviceCollection.AddSingleton(messageSerializer);
			return serviceCollection;
		}

		public static IServiceCollection AddCustomSoapMessageSerializer<TProcessor>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Singleton)
			where TProcessor : class, IXmlSerializationHandler
		{
			serviceCollection.Add(new ServiceDescriptor(typeof(IXmlSerializationHandler), typeof(TProcessor), lifetime));
			return serviceCollection;
		}
	}
}
