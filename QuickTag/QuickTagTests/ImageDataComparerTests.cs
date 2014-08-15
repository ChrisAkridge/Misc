using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickTag.Data;

namespace QuickTagTests
{
	[TestClass]
	public class ImageDataComparerTests
	{
		private const string XFilePath = @"C:\Users\Chris\Documents\Files\Pictures\Desktops\1085.jpg";
		private const string YFilePath = @"C:\Users\Chris\Documents\Files\Pictures\Desktops\b086.jpg";

		[TestMethod]
		public void GreaterThanTest()
		{
			ImageData x = new ImageData(XFilePath);
			ImageData y = new ImageData(YFilePath);
			ImageDataComparer comparer = new ImageDataComparer();
			Assert.IsTrue(comparer.Compare(y, x) > 0);
		}

		[TestMethod]
		public void LessThanTest()
		{
			ImageData x = new ImageData(XFilePath);
			ImageData y = new ImageData(YFilePath);
			ImageDataComparer comparer = new ImageDataComparer();
			Assert.IsTrue(comparer.Compare(x, y) < 0);
		}

		[TestMethod]
		public void EqualTest()
		{
			ImageData x = new ImageData(XFilePath);
			ImageDataComparer comparer = new ImageDataComparer();
			Assert.IsTrue(comparer.Compare(x, x) == 0);
		}
	}
}
