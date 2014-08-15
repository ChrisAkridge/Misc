using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace QuickTag.Data
{
	public sealed class Database : ISerializable
	{
		private List<Folder> folders;

		public ReadOnlyCollection<Folder> Folders
		{
			get
			{
				return folders.AsReadOnly();
			}
		}

		public static Database Empty
		{
			get
			{
				Database result = new Database();
				result.folders = new List<Folder>();
				return result;
			}
		}

		private Database() { }

		public Database(string dbFilePath)
		{
			if (!File.Exists(dbFilePath))
			{
				throw new ArgumentException(string.Format("The database file at {0} does not exist."), "dbFilePath");
			}

			string dbFile = File.ReadAllText(dbFilePath);
			this.Deserialize(dbFile);
		}

		public void AddFolder(Folder folder)
		{
			this.folders.Add(folder);
		}

		public string GetTags(string imagePath)
		{
			ImageData data = this.Search(imagePath);
			if (data != null)
			{
				return String.Join(",", data.Tags);
			}
			return "";
		}

		public ImageData Search(string imagePath)
		{
			foreach (Folder folder in this.folders)
			{
				ImageData result = folder.Search(imagePath);
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		public void PopulateTreeView(TreeView view)
		{
			view.Nodes.Clear();

			TreeNode root = new TreeNode();
			TreeNode node = root;

			foreach (Folder folder in this.folders)
			{
				foreach (ImageData data in folder)
				{
					node = root;
					foreach (string pathSegement in data.ImagePath.Split('\\'))
					{
						node = AddNode(node, pathSegement);
					}
				}
			}

			if (root.Nodes.Count != 0)
			{
				view.Nodes.Add(root.Nodes[0]);
			}
		}

		public void PopulateTagTreeView(TreeView view)
		{
			view.Nodes.Clear();

			Dictionary<string, List<string>> tags = new Dictionary<string, List<string>>();

			foreach (Folder folder in this.folders)
			{
				foreach (ImageData data in folder)
				{
					foreach (string tag in data.Tags)
					{
						if (!tags.ContainsKey(tag))
						{
							tags.Add(tag, new List<string>());
						}
						tags[tag].Add(data.ImagePath);
					}
				}
			}

			foreach (var tag in tags.OrderBy(t => t.Key))
			{
				TreeNode tagNode = new TreeNode(string.Format("{0} ({1})", tag.Key, tag.Value.Count));
				foreach (string imagePath in tag.Value)
				{
					tagNode.Nodes.Add(imagePath);
				}
				view.Nodes.Add(tagNode);
			}
		}

		public object GetSerializableObjects()
		{
			List<object> folderObjects = new List<object>(folders.Count);
			this.folders.ForEach(f => folderObjects.Add(f.GetSerializableObjects()));

			return new
			{
				folders = folderObjects
			};
		}

		public string Serialize()
		{
			return JObject.FromObject(this.GetSerializableObjects()).ToString();
		}

		public void Deserialize(string json)
		{
			this.folders = new List<Folder>();

			JObject obj = JObject.Parse(json);
			JArray folderData = (JArray)obj["folders"];

			foreach (var folder in folderData)
			{
				this.folders.Add(Folder.FromJson(folder.ToString()));
			}
		}

		private TreeNode AddNode(TreeNode node, string key)
		{
			if (node.Nodes.ContainsKey(key))
			{
				return node.Nodes[key];
			}
			else
			{
				return node.Nodes.Add(key, key);
			}
		}

		public static Database FromJson(string json)
		{
			Database result = new Database();
			result.Deserialize(json);
			return result;
		}
	}
}
