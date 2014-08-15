using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickTag.Data;

namespace QuickTagTests
{
	[TestClass]
	public class ImageDataTests
	{
		private const string ValidImagePath = @"C:\Users\Chris\Documents\blockGreen.png";
		[TestMethod]
		public void CreateImageDataTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			Assert.AreEqual(ValidImagePath, data.ImagePath);
		}

		[TestMethod]
		public void Fail_CreateWrongImageDataTest()
		{
			try
			{
				ImageData data = new ImageData(string.Empty, "tag", "test");
			}
			catch (ArgumentException e)
			{
				Assert.IsTrue(true);
			}
		}

		[TestMethod]
		public void VerifyImageDataTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			Assert.AreEqual(2, data.Tags.Count);
			Assert.AreEqual("tag", data.Tags[0]);
			Assert.AreEqual("test", data.Tags[1]);
		}

		[TestMethod]
		public void VerifyImageDataSplitTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag, test");
			Assert.AreEqual(2, data.Tags.Count);
			Assert.AreEqual("tag", data.Tags[0]);
			Assert.AreEqual("test", data.Tags[1]);
		}

		[TestMethod]
		public void VerifyImageDataNoTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath);
			Assert.AreEqual(0, data.Tags.Count);
		}

		[TestMethod]
		public void VerifySerializationTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			string json = data.Serialize();
			ImageData deserializedData = ImageData.FromJson(json);
			Assert.AreEqual(data.ImagePath, deserializedData.ImagePath);
			Assert.AreEqual(deserializedData.Tags[0], "tag");
			Assert.AreEqual(deserializedData.Tags[1], "test");
		}

		[TestMethod]
		public void TaggedTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			Assert.IsTrue(data.Tagged("tag"));
		}

		[TestMethod]
		public void AddTagTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.AddTag("newtag");
			Assert.IsTrue(data.Tagged("newtag"));
		}

		[TestMethod]
		public void AddTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.AddTags("newtag1", "newtag2");
			Assert.IsTrue(data.Tagged("newtag1") && data.Tagged("newtag2"));
		}

		[TestMethod]
		public void AddTagsSplitTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.AddTags("newtag1, newtag2");
			Assert.IsTrue(data.Tagged("newtag1") && data.Tagged("newtag2"));
		}

		[TestMethod]
		public void RemoveTagTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.RemoveTag("tag");
			Assert.IsFalse(data.Tagged("tag"));
		}

		[TestMethod]
		public void RemoveTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.RemoveTags("tag", "test");
			Assert.AreEqual(0, data.Tags.Count);
		}

		[TestMethod]
		public void RemoveTagsSplitTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.RemoveTags("tag, test");
			Assert.AreEqual(0, data.Tags.Count);
		}

		[TestMethod]
		public void SetTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.SetTags("newtag1", "newtag2");
			Assert.IsTrue(data.Tagged("newtag1") && data.Tagged("newtag2"));
		}

		[TestMethod]
		public void SetTagsSplitTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.SetTags("newtag1, newtag2");
			Assert.IsTrue(data.Tagged("newtag1") && data.Tagged("newtag2"));
		}

		[TestMethod]
		public void ClearTagsTest()
		{
			ImageData data = new ImageData(ValidImagePath, "tag", "test");
			data.ClearTags();
			Assert.AreEqual(0, data.Tags.Count);
		}
	}
}
