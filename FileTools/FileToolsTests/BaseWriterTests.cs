using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileTools;

namespace FileToolsTests
{
	[TestClass]
	public class BaseWriterTests
	{
		[TestMethod]
		public void BytesToBinaryString_CommonTest()
		{
			byte[] commonData = new byte[] { 0, 255 };

			string expectedResult = "0000000011111111";
			string actualResult = BaseWriter.BytesToBinaryString(commonData);
			Assert.AreEqual(expectedResult, actualResult);
		}

		[TestMethod]
		public void BytesToBinaryString()
		{
			byte[] data = new byte[] { 0 };

			string expectedResult = "00000000";
			string actualResult = BaseWriter.BytesToBinaryString(data);
			Assert.AreEqual(expectedResult, actualResult);
		}
	}
}
