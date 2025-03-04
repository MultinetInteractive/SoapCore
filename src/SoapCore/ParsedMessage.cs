using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;


namespace SoapCore
{
	public class ParsedMessage : Message
	{
		private readonly MessageHeaders _headers;
		private readonly MessageProperties _properties;
		private readonly MessageVersion _version;
		private readonly XDocument _body;

		/// <summary>
		/// Creates a ParsedMessage from an ASP.NET Core HttpRequest.
		/// </summary>
		public static async Task<ParsedMessage> FromHttpRequestAsync(HttpRequest httpRequest, MessageVersion version)
		{
			if (httpRequest == null) throw new ArgumentNullException(nameof(httpRequest));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var envelope = await ReadHttpRequestBodyAsync(httpRequest);
			var headers = ExtractSoapHeaders(envelope, version);
			var properties = ExtractSoapProperties(httpRequest);
			var body = ExtractSoapBody(envelope, version);

			return new ParsedMessage(headers, properties, version, body);
		}

		public static async Task<ParsedMessage> FromXmlReaderAsync(XmlReader reader, MessageVersion version)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var envelope = XDocument.Load(reader);
			var headers = ExtractSoapHeaders(envelope, version);
			//var properties = ExtractSoapProperties(httpRequest);
			var body = ExtractSoapBody(envelope, version);

			return new ParsedMessage(headers, new MessageProperties(), version, body);
		}

		public ParsedMessage(MessageHeaders headers, MessageProperties properties, MessageVersion version, XDocument body)
		{
			_headers = headers;
			_properties = properties;
			_version = version;
			_body = body;
		}

		public XDocument GetBodyAsXDocument()
		{
			return _body;
		}

		/// <summary>
		/// Reads the HttpRequest body and converts it to an XDocument.
		/// </summary>
		private static async Task<XDocument> ReadHttpRequestBodyAsync(HttpRequest httpRequest)
		{
			if (httpRequest.Body == null || httpRequest.ContentLength == 0)
				return new XDocument();

			httpRequest.EnableBuffering(); // Allows re-reading the stream
			using (var reader = new StreamReader(httpRequest.Body, Encoding.UTF8, true, -1, true))
			{
				string bodyText = await reader.ReadToEndAsync();
				httpRequest.Body.Position = 0; // Reset stream position for further use

				using (var stringReader = new StringReader(bodyText))
				using (var xmlReader = XmlReader.Create(stringReader))
				{
					return XDocument.Load(xmlReader);
				}
			}
		}

		/// <summary>
		/// Extracts SOAP headers from the SOAP message body.
		/// </summary>
		private static MessageHeaders ExtractSoapHeaders(XDocument envelope, MessageVersion version)
		{
			var headers = new MessageHeaders(version);
			var root = envelope.Root;
			if (root == null) return headers;

			XNamespace soapNs = version.Envelope.Namespace();
			var headerNode = root.Element(soapNs + "Header");
			if (headerNode != null)
			{
				foreach (var element in headerNode.Elements())
				{
					bool mustUnderstand = element.Attribute(soapNs + "mustUnderstand")?.Value == "1";
					string actor = element.Attribute(soapNs + "actor")?.Value ?? string.Empty;
					bool relay = element.Attribute(soapNs + "relay")?.Value.ToLower() == "true";

					var header = MessageHeader.CreateHeader(element.Name.LocalName, element.Name.NamespaceName, element.Value, mustUnderstand, actor, relay);

					headers.Add(header);

					if (header.Name.Equals("replyto", StringComparison.OrdinalIgnoreCase))
					{
						headers.ReplyTo = new System.ServiceModel.EndpointAddress(element.Value);
					}
				}
			}

			return headers;
		}

		/// <summary>
		/// Extracts only the SOAP Body content.
		/// </summary>
		private static XDocument ExtractSoapBody(XDocument envelope, MessageVersion version)
		{
			var root = envelope.Root;
			if (root == null) return new XDocument();

			XNamespace soapNs = version.Envelope.Namespace();
			var bodyNode = root.Element(soapNs + "Body");
			if (bodyNode == null) return new XDocument();

			return new XDocument(bodyNode.Elements().FirstOrDefault());
		}

		/// <summary>
		/// Extracts SOAP-specific properties.
		/// </summary>
		private static MessageProperties ExtractSoapProperties(HttpRequest httpRequest)
		{
			var properties = new MessageProperties
			{
				["HttpMethod"] = httpRequest.Method,
				["RequestUri"] = httpRequest.Path + httpRequest.QueryString
			};

			if (httpRequest.ContentType != null)
			{
				properties["Content-Type"] = httpRequest.ContentType;
			}

			return properties;
		}

		public override MessageHeaders Headers => _headers;
		public override MessageProperties Properties => _properties;
		public override MessageVersion Version => _version;

		public override bool IsEmpty => !_body.Elements().Any();

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			_body.WriteTo(writer);
		}

		protected override XmlDictionaryReader OnGetReaderAtBodyContents()
		{
			var reader = _body.CreateReader();
			reader.Read();
			return XmlDictionaryReader.CreateDictionaryReader(reader);
		}

		protected override void OnClose()
		{
			_properties.Dispose();
			base.OnClose();
		}

		protected override MessageBuffer OnCreateBufferedCopy(int maxBufferSize)
		{
			return new ParsedMessageBuffer(this);
		}

		public override string ToString()
		{
			return _body?.ToString() ?? "<Empty Body>";
		}
	}

}
