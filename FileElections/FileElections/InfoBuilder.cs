using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileElections
{
	internal static class InfoBuilder
	{
		private static int foldersProcessed = 0;

		public static TreeNode BuildInfo(string rootFolderPath)
		{
			TreeNode root = new TreeNode(rootFolderPath);

			FillTreeWithDirectories(rootFolderPath, ref root);
			PopulateTreeWithFiles(root);

			return root;
		}

		private static void FillTreeWithDirectories(string folderPath, ref TreeNode node)
		{
			try
			{
				var directories = Directory.GetDirectories(folderPath, "*", SearchOption.TopDirectoryOnly);
				var nodes = directories.Select(d => new TreeNode(d));

				node.Children.AddRange(nodes);

				for (int i = 0; i < node.Children.Count; i++)
				{
					TreeNode childNode = node.Children[i];
					FillTreeWithDirectories(node.Children[i].NodeItem, ref childNode);
					foldersProcessed++;
					if (foldersProcessed % 50 == 0) { Console.WriteLine($"Processed {foldersProcessed} folders."); }
				}
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.WriteLine("Unauthorized");
			}
		}

		private static void PopulateTreeWithFiles(TreeNode node)
		{
			foreach (var directory in node.Children)
			{
				PopulateTreeWithFiles(directory);
			}	

			var files = Directory.GetFiles(node.NodeItem, "*", SearchOption.TopDirectoryOnly);
			node.Children.AddRange(files.Select(f => new TreeNode(f)));
			Console.WriteLine($"Processed {files.Count()} files in {node.NodeItem}");
		}
	}

	internal sealed class TreeNode
	{
		public string NodeItem { get; set; }
		public List<TreeNode> Children { get; set; } = new List<TreeNode>();
		public NodeType NodeType { get; set; }

		public TreeNode(string nodeItem)
		{
			FileAttributes attr = File.GetAttributes(nodeItem);
			NodeType = (attr.HasFlag(FileAttributes.Directory)) ? NodeType.Folder : NodeType.File;
			NodeItem = nodeItem;
		}

		public override string ToString()
		{
			return $"{NodeItem} (children: {Children.Count})";
		}
	}

	internal enum NodeType
	{
		Folder,
		File
	}
}
