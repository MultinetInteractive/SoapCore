using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SoapCore
{
	public class XDocumentXmlReader : XmlReader
	{
		private readonly XDocument _document;
		private readonly XmlReader _reader;

		public XDocumentXmlReader(XDocument document)
		{
			_document = document ?? throw new ArgumentNullException(nameof(document));
			_reader = document.CreateReader();
		}

		public override int ReadContentAsBase64(byte[] buffer, int index, int count)
		{
			string base64String = _reader.Value;
			byte[] data = Convert.FromBase64String(base64String);
			int bytesToCopy = Math.Max(Math.Min(count, data.Length) - index, 0);
			Array.Copy(data, 0, buffer, index, bytesToCopy);
			return bytesToCopy;
		}

		public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
		{
			if (!Read() || _reader.NodeType != XmlNodeType.Text)
			{
				return 0;
			}

			return ReadContentAsBase64(buffer, index, count);
		}

		public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
		{
			string hexString = _reader.Value;
#if NET8_0_OR_GREATER
			byte[] data = Convert.FromHexString(hexString);
#else
			byte[] data = Enumerable.Range(0, hexString.Length)
                      .Where(x => x % 2 == 0)
                      .Select(x => Convert.ToByte(hexString.Substring(x, 2), 16))
                      .ToArray();
#endif
			int bytesToCopy = Math.Max(Math.Min(count, data.Length) - index, 0);
			Array.Copy(data, 0, buffer, index, bytesToCopy);
			return bytesToCopy;
		}

		public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
		{
			if (!Read() || _reader.NodeType != XmlNodeType.Text)
			{
				return 0;
			}

			return ReadContentAsBinHex(buffer, index, count);
		}

		public override bool Read() => _reader.Read();
		public override string GetAttribute(int i) => _reader.GetAttribute(i);
		public override string GetAttribute(string name) => _reader.GetAttribute(name);
		public override string GetAttribute(string name, string namespaceURI) => _reader.GetAttribute(name, namespaceURI);
		public override bool MoveToAttribute(string name) => _reader.MoveToAttribute(name);
		public override bool MoveToAttribute(string name, string ns) => _reader.MoveToAttribute(name, ns);
		public override bool MoveToFirstAttribute() => _reader.MoveToFirstAttribute();
		public override bool MoveToNextAttribute() => _reader.MoveToNextAttribute();
		public override bool MoveToElement() => _reader.MoveToElement();
		public override void Close() => _reader.Close();
		public override string LookupNamespace(string prefix) => _reader.LookupNamespace(prefix);
		public override void ResolveEntity() => _reader.ResolveEntity();
		public override bool ReadAttributeValue() => _reader.ReadAttributeValue();

		public override XmlNodeType NodeType => _reader.NodeType;
		public override string Name => _reader.Name;
		public override string LocalName => _reader.LocalName;
		public override string NamespaceURI => _reader.NamespaceURI;
		public override string Prefix => _reader.Prefix;
		public override bool HasValue => _reader.HasValue;
		public override string Value => _reader.Value;
		public override int Depth => _reader.Depth;
		public override string BaseURI => _reader.BaseURI;
		public override bool IsEmptyElement => _reader.IsEmptyElement;
		public override int AttributeCount => _reader.AttributeCount;
		public override bool EOF => _reader.EOF;
		public override ReadState ReadState => _reader.ReadState;
		public override XmlNameTable NameTable => _reader.NameTable;
	}
}
