using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickTag
{
	public partial class FolderSelector : Form
	{
		public FolderSelector()
		{
			InitializeComponent();
		}

		private void ButtonCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void ButtonSelectFolder_Click(object sender, EventArgs e)
		{
			if (this.FBDFolderSelector.ShowDialog() == DialogResult.OK)
			{
				this.TextBoxPath.Text = this.FBDFolderSelector.SelectedPath;
			}
		}
	}
}
