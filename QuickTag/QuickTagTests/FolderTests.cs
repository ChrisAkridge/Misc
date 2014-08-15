using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QuickTag.Data;

namespace QuickTagTests
{
	[TestClass]
	public class FolderTests
	{
		[TestMethod]
		public void CreateFromFolderTest()
		{
			string folderPath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers";
			Folder result = Folder.CreateFromFolder(folderPath);
			// TODO: assert something here
		}

		[TestMethod]
		public void GetTagsTest()
		{
			string folderPath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers";
			Folder folder = Folder.CreateFromFolder(folderPath);

			foreach (ImageData data in folder)
			{
				data.SetTags("tag");
			}

			var tags = folder.GetTags();
			foreach(var tag in tags)
			{
				Console.WriteLine("{0} ({1})", tag.Key, tag.Value);
			}
		}

		[TestMethod]
		public void SortedInsertTest()
		{
			string folderPath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers";
			string filePath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers\c152.jpg";
			Folder folder = Folder.CreateFromFolder(folderPath);
			int index = folder.IndexOf(new ImageData(filePath));
			folder.Remove(new ImageData(filePath));
			folder.Add(new ImageData(filePath));
			Assert.AreEqual(index, folder.IndexOf(new ImageData(filePath)));
		}

		[TestMethod]
		public void SerializeTest()
		{
			string folderPath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers";
			Folder folder = Folder.CreateFromFolder(folderPath);
			string json = folder.Serialize();
			Assert.IsNotNull(json);
		}

		[TestMethod]
		public void DeserializeTest()
		{
			string folderPath = @"C:\Users\Chris\Documents\Files\Pictures\Wallpapers";
			Folder folder = Folder.CreateFromFolder(folderPath);

			Folder folderB = Folder.FromJson(folder.Serialize());
			Assert.AreEqual(folder.Count, folderB.Count);
		}
	}
}
