using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoapCore.Tests
{
	[TestClass]
	public class XDocumentXmlReaderTests
	{
		[TestMethod]
		public void TestReadBase64()
		{
			var strBase64 = "QkVHSU4KQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhCkVORA==";

			var xml = @$"
<Request>
	<Base64Data>
{strBase64}
	</Base64Data>
</Request>
";
			var xdoc = XDocument.Parse(xml);
			var reader = new XDocumentXmlReader(xdoc);

			reader.Read();
			reader.Read();

			var buffer = new byte[1024];
			var ms = new MemoryStream();
			while (true)
			{
				var i = reader.ReadElementContentAsBase64(buffer, 0, 1024);
				ms.Write(buffer, 0, i);

				if (i < buffer.Length)
				{
					break;
				}
			}

			ms.Position = 0;

			var b64buffer = Convert.FromBase64String(strBase64);

			Assert.IsTrue(b64buffer.SequenceEqual(ms.ToArray()));

			Assert.AreEqual(1336, ms.Length);
		}
	}
}
