using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public partial class FolderSelect : Form
	{
		public FolderSelect()
		{
			InitializeComponent();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				this.folderBrowserDialog1.SelectedPath = this.textBox1.Text;
			}
			catch
			{
				this.folderBrowserDialog1.SelectedPath = "C:\\";
			}

			if (this.folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				this.textBox1.Text = this.folderBrowserDialog1.SelectedPath;
			}
		}
	}
}
