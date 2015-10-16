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
    public partial class Form1 : Form
    {
		private FolderSelect folderSelect;
        private BracketedTournament<string> files;

        public Form1()
        {
            InitializeComponent();
			folderSelect = new FolderSelect();
        }

        private List<string> GetFilesInFolder(string folderPath)
        {
			var result = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Where(f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".gif") || f.ToLower().EndsWith(".bmp") || f.ToLower().EndsWith(".jpeg"))).ToList();
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (folderSelect.ShowDialog() == DialogResult.OK)
            {
				files = new BracketedTournament<string>(GetFilesInFolder(folderSelect.textBox1.Text));
				files.WinnerChosenEvent += new WinnerChosenEventHandler<string>(files_WinnerChosenEvent);
				StartRound();
            }
            else
            {
                Application.Exit();
            }
        }

        void files_WinnerChosenEvent(string winner, string standings)
        {
            var winnerForm = new WinnerForm();
            winnerForm.ShowWinner(winner, standings);
            winnerForm.ShowDialog();
            Application.Exit();
        }

        private void DisplayPictures(string picture1, string picture2)
        {
			button1.Image = Image.FromFile(picture1);
			button2.Image = Image.FromFile(picture2);

            if (button1.Image.Width < button1.Width && button1.Image.Height < button1.Height) 
            {
                button1.SizeMode = PictureBoxSizeMode.CenterImage; 
            }
            else 
            {
                button1.SizeMode = PictureBoxSizeMode.Zoom; 
            }

            if (button2.Image.Width < button2.Width && button2.Image.Height < button2.Height)
            {
                button2.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                button2.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private void StartRound()
        {
            if (files == null)
            {
                throw new Exception("Files list uninitialized");
            }

			files.StartRound();
            if (files.currentRoundNumber == 1)
            {
                ////this.files.ShuffleTeams();
            }
			label2.Text = string.Format("Round {0} ({1} matches remaining)", files.currentRoundNumber, files.currentRound.matchesRemaining);
			progressBar1.Maximum = files.currentRound.matchesRemaining;
			progressBar1.Value = 0;

            var nextMatch = files.currentRound.GetNextMatch();
			DisplayPictures(nextMatch.team1, nextMatch.team2);
        }

        private void SelectWinner(int teamNumber)
        {
			files.currentRound.SelectNextMatchWinner(teamNumber);
			progressBar1.Value++;
			label2.Text = string.Format("Round {0} ({1} matches remaining)", files.currentRoundNumber, (files.currentRound != null) ? files.currentRound.matchesRemaining : 0);

            if (files.currentRound == null)
            {
				// The round has concluded, load the next one
				files.StartRound();
				progressBar1.Maximum = files.currentRound.matchesRemaining;
				progressBar1.Value = 0;
				label2.Text = string.Format("Round {0} ({1} matches remaining)", files.currentRoundNumber, (files.currentRound != null) ? files.currentRound.matchesRemaining : 0);
            }

            var nextMatch = files.currentRound.GetNextMatch();
            if (nextMatch == null) return;
			DisplayPictures(nextMatch.team1, nextMatch.team2);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
			SelectWinner(1);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
			SelectWinner(2);
        }
    }
}
