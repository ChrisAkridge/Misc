using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QuickTag.Data;

namespace QuickTag
{
	public partial class MainForm : Form
	{
		private StartForm launcher;
		private Database database;
		private string dbPath;
		private string currentImage;
		private bool imageSelectedByArrowKeys;

		public MainForm(StartForm launcher)
		{
			InitializeComponent();
			this.launcher = launcher;
		}

		public void Initialize(string dbPath)
		{
			this.dbPath = dbPath;

			string json = File.ReadAllText(dbPath);
			this.database = Database.FromJson(json);

			this.Text = string.Format("QuickTag - {0}", Path.GetFileNameWithoutExtension(dbPath));
			this.database.PopulateTreeView(this.TreeViewFiles);
			this.database.PopulateTagTreeView(this.TreeViewTags);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.launcher.Close();
			// TODO: make TreeViewFiles actually contain all the files
			// maybe make it wider
			// wire up the other buttons
			// replace TreeViewPopulate with the top answer here: http://stackoverflow.com/questions/673931/file-system-treeview/674119#674119
		}

		private void ButtonAddFolders_Click(object sender, EventArgs e)
		{
			FolderSelector selector = new FolderSelector();
			if (selector.ShowDialog() == DialogResult.OK)
			{
				string path = selector.TextBoxPath.Text;
				Folder newFolder = Folder.CreateFromFolder(path);
				this.database.AddFolder(newFolder);
				this.database.PopulateTreeView(this.TreeViewFiles);
			}
		}

		private void ButtonSaveAndClose_Click(object sender, EventArgs e)
		{
			File.WriteAllText(dbPath, this.database.Serialize());
			this.Close();
		}

		private void ButtonSetTags_Click(object sender, EventArgs e)
		{
			if (this.currentImage == null) return;

			string[] tags = this.TextBoxImageTags.Text.Split(',');
			ImageData data = this.database.Search(this.currentImage);
			data.SetTags(tags);
			this.database.PopulateTagTreeView(this.TreeViewTags);
		}

		private void ChangeSelectedImage(int direction)
		{
			if (this.TreeViewFiles.Focused || this.TreeViewTags.Focused || this.TextBoxImageTags.Focused ||
				this.currentImage == null || this.TreeViewFiles.Nodes.Count == 0)
			{
				return;
			}

			TreeNode current = this.SearchFileNode(this.currentImage);
			if (direction == -1)
			{
				if (current.LastNode != null)
				{
					this.TreeViewFiles.SelectedNode = current.Parent.LastNode;
				}
				else
				{
					this.TreeViewFiles.SelectedNode = current.LastNode;
				}
			}
			else if (direction == 1)
			{
				if (current.NextNode == null)
				{
					this.TreeViewFiles.SelectedNode = current.Parent.FirstNode;
				}
				else
				{
					this.TreeViewFiles.SelectedNode = current.NextNode;
				}
			}

			this.TreeViewFiles_AfterSelect(null, null);
			this.PictureBox.Focus();
		}

		private void PictureBox_Click(object sender, EventArgs e)
		{
			this.PictureBox.Focus();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Left)
			{
				this.ChangeSelectedImage(-1);
			}
			else if (keyData == Keys.Right)
			{
				this.ChangeSelectedImage(1);
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private TreeNode SearchFileNode(string imagePath)
		{
			string[] pathSegments = imagePath.Split('\\');
			TreeNode current = this.TreeViewFiles.TopNode;

			foreach (string segment in pathSegments)
			{
				current = SearchTreeNode(current, segment);
			}

			return current;
		}

		private TreeNode SearchTreeNode(TreeNode startNode, string searchTerm)
		{
			if (startNode.Text == searchTerm) return startNode;

			foreach (TreeNode node in startNode.Nodes)
			{
				if (node.Text == searchTerm)
				{
					return node;
				}
			}
			return null;
		}

		private void TextBoxImageTags_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				this.ButtonSetTags_Click(this.ButtonSetTags, new EventArgs());
			}
		}

		private void TreeViewFiles_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (!imageSelectedByArrowKeys)
			{
				// We only want to do this if the user hasn't used the left/right arrow keys to select the image
				// We change the selected item every time the image changes, but we don't want to reload the image
				// when this event handler is called.

				// Construct the full path using the parents of the current node.
				StringBuilder pathBuilder = new StringBuilder();
				TreeNode selectedNode = this.TreeViewFiles.SelectedNode;
				pathBuilder.Append("\\" + selectedNode.Text); // this should be optimized to a Concat call

				TreeNode parent = selectedNode.Parent;
				while (parent != null)
				{
					pathBuilder.Insert(0, "\\" + parent.Text);
					parent = parent.Parent; // yo dawg
				}

				string path = pathBuilder.ToString().Substring(1);
				// Check that the file exists - if not, we probably selected a folder or disk.
				if (!File.Exists(path)) { return; }

				this.currentImage = path;
				this.PictureBox.Image = Image.FromFile(this.currentImage);
				this.TextBoxImageTags.Text = this.database.GetTags(path);
			}
			else
			{
				imageSelectedByArrowKeys = false;
			}
		}

		private void TreeViewTags_AfterSelect(object sender, TreeViewEventArgs e)
		{
			TreeNode currentNode = this.TreeViewTags.SelectedNode;
			if (currentNode.Parent != null)
			{
				// If the parent IS null, we've selected a tag
				string path = currentNode.Text;
				this.currentImage = path;
				this.PictureBox.Image = Image.FromFile(path);
				this.TextBoxImageTags.Text = this.database.GetTags(path);
				this.imageSelectedByArrowKeys = false;
			}
		}
	}
}
