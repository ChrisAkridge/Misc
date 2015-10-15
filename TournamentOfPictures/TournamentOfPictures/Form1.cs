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
			this.folderSelect = new FolderSelect();
        }

        private List<string> GetFilesInFolder(string folderPath)
        {
            List<string> result = new List<string>();
            foreach (string file in Directory.GetFiles(folderPath))
            {
                result.Add(file);
            }
            //foreach (string directory in Directory.GetDirectories(folderPath))
            //{
            //    result.AddRange(this.GetFilesInFolder(directory));
            //}
            // TODO: debug the next line
            result = result.Where(file => (file.ToLower().EndsWith(".png") || file.ToLower().EndsWith(".jpg") || file.ToLower().EndsWith(".gif") || file.ToLower().EndsWith(".bmp") || file.ToLower().EndsWith(".jpeg"))).ToList();
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (this.folderSelect.ShowDialog() == DialogResult.OK)
            {
                this.files = new BracketedTournament<string>(this.GetFilesInFolder(this.folderSelect.textBox1.Text));
                this.files.WinnerChosenEvent += new WinnerChosenEventHandler<string>(files_WinnerChosenEvent);
                this.StartRound();
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
            this.button1.Image = Image.FromFile(picture1);
            this.button2.Image = Image.FromFile(picture2);

            if (this.button1.Image.Width < button1.Width && this.button1.Image.Height < button1.Height) 
            {
                button1.SizeMode = PictureBoxSizeMode.CenterImage; 
            }
            else 
            {
                button1.SizeMode = PictureBoxSizeMode.Zoom; 
            }

            if (this.button2.Image.Width < button2.Width && this.button2.Image.Height < button2.Height)
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
            if (this.files == null)
            {
                throw new Exception("Files list uninitialized");
            }

            this.files.StartRound();
            if (this.files.currentRoundNumber == 1)
            {
                ////this.files.ShuffleTeams();
            }
            this.label2.Text = string.Format("Round {0} ({1} matches remaining)", this.files.currentRoundNumber, this.files.currentRound.matchesRemaining);
            this.progressBar1.Maximum = this.files.currentRound.matchesRemaining;
            this.progressBar1.Value = 0;

            var nextMatch = this.files.currentRound.GetNextMatch();
            this.DisplayPictures(nextMatch.team1, nextMatch.team2);
        }

        private void SelectWinner(int teamNumber)
        {
            this.files.currentRound.SelectNextMatchWinner(teamNumber);
            this.progressBar1.Value++;
            this.label2.Text = string.Format("Round {0} ({1} matches remaining)", this.files.currentRoundNumber, (this.files.currentRound != null) ? this.files.currentRound.matchesRemaining : 0);

            if (this.files.currentRound == null)
            {
                // The round has concluded, load the next one
                this.files.StartRound();
                this.progressBar1.Maximum = this.files.currentRound.matchesRemaining;
                this.progressBar1.Value = 0;
                this.label2.Text = string.Format("Round {0} ({1} matches remaining)", this.files.currentRoundNumber, (this.files.currentRound != null) ? this.files.currentRound.matchesRemaining : 0);
            }

            var nextMatch = this.files.currentRound.GetNextMatch();
            if (nextMatch == null) return;
            this.DisplayPictures(nextMatch.team1, nextMatch.team2);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            this.SelectWinner(1);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.SelectWinner(2);
        }
    }
}
