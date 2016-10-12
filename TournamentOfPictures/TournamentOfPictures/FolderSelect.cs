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
			DialogResult = DialogResult.OK;
		}

		private void button2_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				folderBrowserDialog1.SelectedPath = textBox1.Text;
			}
			catch
			{
				folderBrowserDialog1.SelectedPath = "C:\\";
			}

			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
		}

		private void ButtonOpenTournament_Click(object sender, EventArgs e)
		{
			if (OFDOpenTournament.ShowDialog() == DialogResult.OK)
			{
				DialogResult = DialogResult.Yes;
				textBox1.Text = OFDOpenTournament.FileName;
			}
		}
	}
}
