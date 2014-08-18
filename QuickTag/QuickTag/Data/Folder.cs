using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QuickTag.Data
{
	public sealed class Folder : ISerializable
	{
		private List<ImageData> images;
		private ImageDataComparer comparer;

		public string Name { get; set; }
		public string Path { get; set; }

		public int Count
		{
			get
			{
				return this.images.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		public ImageData this[int index]
		{
			get
			{
				return this.images[index];
			}
		}

		public ImageData this[string filePath]
		{
			get
			{
				return this.images.Where(i => i.ImagePath == filePath).FirstOrDefault();
			}
		}

		private Folder()
		{
			this.images = new List<ImageData>();
			this.comparer = new ImageDataComparer();
		}

		public void Add(ImageData item)
		{
			if (this.images.Count == 0)
			{
				this.images.Add(item);
				return;
			}

			var index = this.images.BinarySearch(item, this.comparer);
			if (index < 0)
			{
				this.images.Insert(~index, item);
			}
		}

		public void Clear()
		{
			this.images.Clear();
		}

		public bool Contains(ImageData item)
		{
			return this.images.Contains(item);
		}

		public void CopyTo(ImageData[] array, int index)
		{
			this.images.CopyTo(array, index);
		}

		public IEnumerator<ImageData> GetEnumerator()
		{
			return this.images.GetEnumerator();
		}

		public int IndexOf(ImageData item)
		{
			return this.images.IndexOf(item);
		}

		public bool Remove(ImageData item)
		{
			return this.images.Remove(item);
		}

		public void RemoveAt(int index)
		{
			this.images.RemoveAt(index);
		}

		public ImageData Search(string imagePath)
		{
			var matches = this.images.Where(i => i.ImagePath == imagePath);
			return matches.FirstOrDefault();
		}

		public void Update()
		{
			var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ ".bmp", ".jpg", ".jpeg", ".gif", ".png", ".tiff", ".svg", ".webm" };

			var currentFileNames = this.images.Select(i => i.ImagePath).ToList();
			var newFileNames = Directory.EnumerateFiles(this.Path, "*", SearchOption.AllDirectories).Where(f => extensions.Contains(System.IO.Path.GetExtension(f))).ToList();
			var newFilesInFolder = newFileNames.Except(currentFileNames).ToList();

			// Add the new files to the folder.
			foreach (string newFile in newFilesInFolder)
			{
				this.images.Add(new ImageData(newFile));
			}

			var deletedFiles = currentFileNames.Except(newFileNames).ToList();
			List<ImageData> deletedImageData = new List<ImageData>(deletedFiles.Count);
			foreach (string deletedFile in deletedFiles)
			{
				deletedImageData.Add(this.images.Where(i => i.ImagePath == deletedFile).FirstOrDefault());
			}

			// Remove the deleted files from the folder.
			foreach (ImageData data in deletedImageData)
			{
				this.Remove(data);
			}
		}

		public Dictionary<string, int> GetTags()
		{
			if (this.images.Count == 0)
			{
				return new Dictionary<string, int>();
			}

			Dictionary<string, int> result = new Dictionary<string, int>();
			foreach (ImageData data in this.images)
			{
				foreach (string tag in data.Tags)
				{
					if (!result.ContainsKey(tag))
					{
						result.Add(tag, 0);
					}

					result[tag]++;
				}
			}

			return result;
		}

		public object GetSerializableObjects()
		{
			List<object> imageObjects = new List<object>(this.images.Count);
			images.ForEach(i => imageObjects.Add(i.GetSerializableObjects()));

			return new
			{
				name = this.Name,
				path = this.Path,
				images = imageObjects
			};
		}

		public string Serialize()
		{
			return JObject.FromObject(this.GetSerializableObjects()).ToString();
		}

		public void Deserialize(string json)
		{
			this.images = new List<ImageData>();

			JObject obj = JObject.Parse(json);

			this.Name = (string)obj["name"];
			this.Path = (string)obj["path"];

			JArray imagesData = (JArray)obj["images"];
			foreach (var image in imagesData)
			{
				this.images.Add(ImageData.FromJson(image.ToString()));
			}
		}

		public static Folder CreateFromFolder(string folderPath)
		{
			if (!Directory.Exists(folderPath))
			{
				throw new ArgumentException(string.Format("The folder at {0} does not exist.", folderPath), "folderPath");
			}

			var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase){ ".bmp", ".jpg", ".jpeg", ".gif", ".png", ".tiff", ".svg", ".webm" };

			Folder result = new Folder();
			result.Path = folderPath;
			result.Name = folderPath.Split('\\').Last();
			var imageFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Where(f => extensions.Contains(System.IO.Path.GetExtension(f)));

			foreach (string file in imageFiles)
			{
				ImageData data = new ImageData(file);
				result.images.Add(data);
			}

			result.images = result.images.OrderBy(i => System.IO.Path.GetFileName(i.ImagePath)).ToList();
			return result;
		}

		public static Folder FromJson(string json)
		{
			Folder result = new Folder();
			result.Deserialize(json);
			return result;
		}
	}
}
