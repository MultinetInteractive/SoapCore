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
		private readonly bool _isEmpty;

		public static ParsedMessage FromXmlReaderAsync(XmlReader reader, MessageVersion version)
		{
			if (reader == null) throw new ArgumentNullException(nameof(reader));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var envelope = XDocument.Load(reader);
			var headers = ExtractSoapHeaders(envelope, version);
			//var properties = ExtractSoapProperties(httpRequest);
			(var body, var isEmpty) = ExtractSoapBody(envelope, version);

			return new ParsedMessage(headers, new MessageProperties(), version, body, isEmpty);
		}

		public ParsedMessage(MessageHeaders headers, MessageProperties properties, MessageVersion version, XDocument body, bool isEmpty)
		{
			_headers = headers;
			_properties = properties;
			_version = version;
			_body = body;
			_isEmpty = isEmpty;
		}

		public XDocument GetBodyAsXDocument()
		{
			return _body;
		}

		/// <summary>
		/// Extracts SOAP headers from the SOAP message body.
		/// </summary>
		private static MessageHeaders ExtractSoapHeaders(XDocument envelope, MessageVersion version)
		{
			var headers = new MessageHeaders(version);
			var root = envelope.Root;
			if (root == null)
			{
				return headers;
			}

			XNamespace soapNs = version.Envelope.Namespace();
			var headerNode = root.Element(soapNs + "Header");
			if (headerNode != null)
			{
				foreach (var element in headerNode.Elements())
				{
					var header = new ParsedMessageHeader(element.Name.LocalName, element.Name.NamespaceName, element.Elements().ToArray(), element.Value, element.Attributes().ToArray());
					headers.Add(header);
				}
			}

			return headers;
		}

		/// <summary>
		/// Extracts only the SOAP Body content.
		/// </summary>
		private static (XDocument, bool isEmpty) ExtractSoapBody(XDocument envelope, MessageVersion version)
		{
			var root = envelope.Root;
			if (root == null)
			{
				return (new XDocument(), true);
			}

			XNamespace soapNs = version.Envelope.Namespace();
			var bodyNode = root.Element(soapNs + "Body");
			if (bodyNode == null)
			{
				return (new XDocument(), true);
			}

			//return new XDocument(bodyNode.Elements().FirstOrDefault());
			return (new XDocument(bodyNode), bodyNode.IsEmpty);
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


		public override bool IsEmpty => _isEmpty;

		protected override void OnWriteBodyContents(XmlDictionaryWriter writer)
		{
			using (var reader = GetReaderAtBodyContents())
			{
				writer.WriteNode(reader, true);
			}
		}

		protected override XmlDictionaryReader OnGetReaderAtBodyContents()
		{
			var reader = new XDocumentXmlReader(_body);

			//var reader = XmlReader.Create(new StringReader(_body.ToString()));
			XNamespace soapNs = _version.Envelope.Namespace();

			while (reader.Read()) // Advance through the document
			{
				if (reader.NodeType == XmlNodeType.Element && reader.LocalName == "Body" && reader.NamespaceURI.Equals(soapNs.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
			}

			while (reader.Read() && reader.NodeType != XmlNodeType.Element && reader.NodeType != XmlNodeType.EndElement)
			{
			}

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
