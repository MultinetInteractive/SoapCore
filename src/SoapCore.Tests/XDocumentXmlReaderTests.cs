using System;
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
			var xml = @"
<Request>
	<Base64Data>
QkVHSU4KQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhQmxhYmxhYmxhbGJsYUJsYWJsYWJsYWxibGFCbGFibGFibGFsYmxhCkVORA==
	</Base64Data>
</Request>
";
			var xdoc = XDocument.Parse(xml);
			var reader = new XDocumentXmlReader(xdoc);

			reader.Read();
			reader.Read();
			reader.Read();

			var buffer = new byte[1024];
			var ms = new MemoryStream();
			var start = 0;
			while (true)
			{
				var i = reader.ReadContentAsBase64(buffer, start, 1024);
				ms.Write(buffer, 0, i);
				start += i;

				if (i < buffer.Length)
				{
					break;
				}
			}

			ms.Position = 0;

			Assert.AreEqual(1336, ms.Length);
		}
	}
}
