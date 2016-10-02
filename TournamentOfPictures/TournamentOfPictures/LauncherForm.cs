using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public partial class LauncherForm : Form
	{
		private string folderPath;

		public LauncherForm()
		{
			InitializeComponent();
		}

		private void LauncherForm_Load(object sender, EventArgs e)
		{
			FolderSelect folderSelect = new FolderSelect();
			DialogResult result = folderSelect.ShowDialog();
			if (result == DialogResult.Cancel)
			{
				Close();
			}
			else if (result == DialogResult.OK)
			{
				folderPath = folderSelect.textBox1.Text;
				SetFolderDetails();
			}
		}

		private void SetFolderDetails()
		{
			int fileCount = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly).Count();
			int nLogN = (int)(fileCount * Math.Log(fileCount, 2d));
			long nSquared = fileCount * fileCount;

			LabelFolderDetails.Text = $"Selected Folder \"{Path.GetDirectoryName(folderPath)}\"\r\nPictures: {fileCount}\r\nn-1: {fileCount - 1}\r\nn log n: {nLogN}\r\nn²: {nSquared}";
		}

		private void ButtonLaunchBracketedTournament_Click(object sender, EventArgs e)
		{
			Form1 tournamentForm = new Form1(folderPath);
			tournamentForm.FormClosed += (s, args) => Close();
			Hide();
			tournamentForm.Show();
		}

		private void ButtonLaunchRating_Click(object sender, EventArgs e)
		{
			PictureRatingSelector selectorForm = new PictureRatingSelector(folderPath, false);
			selectorForm.FormClosed += (s, args) => Close();
			Hide();
			selectorForm.Show();
		}

		private void ButtonLaunchRecursiveRating_Click(object sender, EventArgs e)
		{
			PictureRatingSelector selectorForm = new PictureRatingSelector(folderPath, true);
			selectorForm.FormClosed += (s, args) => Close();
			Hide();
			selectorForm.Show();
		}

		private void ButtonLaunchQuantificationTournament_Click(object sender, EventArgs e)
		{
			QuantificationForm quantificationForm = new QuantificationForm(folderPath);
			quantificationForm.FormClosed += (s, args) => Close();
			Hide();
			quantificationForm.Show();
		}
	}
}
