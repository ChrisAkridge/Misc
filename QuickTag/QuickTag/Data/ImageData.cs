using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace QuickTag.Data
{
	public sealed class ImageData : ISerializable
	{
		private List<string> tags;

		public string ImagePath { get; private set; }
		public ReadOnlyCollection<string> Tags
		{
			get
			{
				if (this.tags == null)
				{
					this.tags = new List<string>();
				}
				return this.tags.AsReadOnly();
			}
		}

		private ImageData() { }

		public ImageData(string imagePath, string tags) : this(imagePath, FormatTags(tags.Split(','))) { }

		public ImageData(string imagePath, params string[] tags)
		{
			if (!File.Exists(imagePath))
			{
				throw new ArgumentException(string.Format("The file at {0} does not exist.", imagePath), "imagePath");
			}

			this.ImagePath = imagePath;
			this.tags = tags.ToList();
		}

		public object GetSerializableObjects()
		{
			return new
			{
				path = this.ImagePath,
				tags = this.Tags
			};
		}

		public string Serialize()
		{
			return JObject.FromObject(this.GetSerializableObjects()).ToString();
		}

		public void Deserialize(string json)
		{
			JObject obj = JObject.Parse(json);
			this.ImagePath = (string)obj["path"];

			JArray tagArray = (JArray)obj["tags"];
			this.tags = new List<string>();
			foreach (var tag in tagArray)
			{
				this.tags.Add((string)tag);
			}
		}

		public void AddTag(string tag)
		{
			if (this.tags == null)
			{
				this.tags = new List<string>();
			}

			this.tags.Add(tag);
		}

		public void AddTags(params string[] tags)
		{
			foreach (string tag in tags)
			{
				this.AddTag(tag);
			}
		}

		public void AddTags(string tags)
		{
			this.AddTags(FormatTags(tags.Split(',')));
		}

		public void ClearTags()
		{
			this.tags.Clear();
		}

		public void RemoveTag(string tag)
		{
			if (this.tags == null)
			{
				return;
			}

			this.tags.Remove(tag);
		}

		public void RemoveTags(params string[] tags)
		{
			foreach (string tag in tags)
			{
				this.RemoveTag(tag);
			}
		}

		public void RemoveTags(string tags)
		{
			this.RemoveTags(FormatTags(tags.Split(',')));
		}

		public void SetTags(params string[] tags)
		{
			this.tags = tags.ToList();
		}

		public void SetTags(string tags)
		{
			this.tags = FormatTags(tags.Split(',')).ToList();
		}

		public bool Tagged(string tag)
		{
			if (this.tags == null)
			{
				return false;
			}

			return this.tags.Contains(tag);
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", this.ImagePath, this.tags.ToArray());
		}

		public static ImageData FromJson(string json)
		{
			ImageData result = new ImageData();
			result.Deserialize(json);
			return result;
		}

		private static string[] FormatTags(string[] input)
		{
			string[] result = new string[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				result[i] = input[i].Trim().ToLowerInvariant();
			}
			return result;
		}
	}
}
