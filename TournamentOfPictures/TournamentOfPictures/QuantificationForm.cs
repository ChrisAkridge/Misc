using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public partial class QuantificationForm : Form
	{
		private string folderPath;
		private QuantificationTournament<string> tournament;
		private int displayedScore = 0;

		public QuantificationForm(string folderPath)
		{
			InitializeComponent();

			this.folderPath = folderPath;
			tournament = new QuantificationTournament<string>(
			System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly).Where(
			f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg") || f.ToLower().EndsWith(".bmp")
			|| f.ToLower().EndsWith(".gif"))));
			tournament.WinnerSelected += (winner, standings) =>
			{
				var winnerForm = new WinnerForm();
				winnerForm.ShowWinner(winner.Item, standings);
				winnerForm.ShowDialog();
				Application.Exit();
			};

			Progress.Maximum = tournament.Count - 1;
			UpdateInfo();
		}

		private void UpdateInfo()
		{
			Picture.Image = Image.FromFile(tournament.Current);
			LabelInfo.Text = $"Picture {tournament.CurrentIndex + 1} of {tournament.Count}.";
			Progress.Value = tournament.CurrentIndex;
		}

		private void ButtonMinusOne_Click(object sender, EventArgs e)
		{
			displayedScore--;
			LabelScore.Text = displayedScore.ToString();
		}

		private void ButtonPlusOne_Click(object sender, EventArgs e)
		{
			displayedScore++;
			LabelScore.Text = displayedScore.ToString();
		}

		private void ButtonSubmit_Click(object sender, EventArgs e)
		{
			tournament.ScoreCurrent(displayedScore);
			displayedScore = 0;
			LabelScore.Text = displayedScore.ToString();
			UpdateInfo();
		}
	}
}
