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
		private string folderPath;
		private BracketedTournament<string> files;
		private int roundsCompleted;

		public InitialTeamOrder TeamOrder { get; set; }

        public Form1(string folderPath)
        {
            InitializeComponent();
			this.folderPath = folderPath;
        }

		public Form1(BracketedTournament<string> tournament)
		{
			InitializeComponent();
			files = tournament;
		}

        private List<string> GetFilesInFolder(string folderPath)
        {
			var result = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly).Where(f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".gif") || f.ToLower().EndsWith(".bmp") || f.ToLower().EndsWith(".jpeg"))).ToList();
            return result;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
			if (files == null)
			{ 
				files = new BracketedTournament<string>(GetFilesInFolder(folderPath), TeamOrder);
				StartRound();
			}
			else
			{
				DisplayPictures(files.CurrentRound.CurrentMatch.Team1, files.CurrentRound.CurrentMatch.Team2);
				UpdateLabel();
				SetProgress();
			}
			files.WinnerChosenEvent += Files_WinnerChosenEvent;
        }

		private void Files_WinnerChosenEvent(string winner, IEnumerable<ScoredItem<string>> standings)
		{
			var winnerForm = new WinnerForm();
			winnerForm.ShowWinner(winner, standings, files.GetPlaylistOrder());
			winnerForm.ShowDialog();
			Application.Exit();
		}

		private void DisplayPictures(string picture1, string picture2)
        {
			var oldImage1 = button1.Image;
			var oldImage2 = button2.Image;

			button1.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(picture1)));
			button2.Image = Image.FromStream(new MemoryStream(File.ReadAllBytes(picture2)));

			oldImage1?.Dispose();
			oldImage2?.Dispose();

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
            if (files.CurrentRoundNumber == 1)
            {
                ////this.files.ShuffleTeams();
            }
			UpdateLabel();
			SetProgress();

            var match = files.CurrentRound.CurrentMatch;
			DisplayPictures(match.Team1, match.Team2);
        }

        private void SelectWinner(int teamNumber)
        {
			files.CurrentRound.SelectMatchWinner(teamNumber);
			UpdateLabel();

            if (files.CurrentRound == null)
            {
				// The round has concluded, load the next one
				files.StartRound();
				UpdateLabel();
            }
			roundsCompleted++;
			SetProgress();

            var match = files.CurrentRound.CurrentMatch;
            if (match == null) return;
			DisplayPictures(match.Team1, match.Team2);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
			SelectWinner(1);
			ButtonUndo.Enabled = true;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
			SelectWinner(2);
			ButtonUndo.Enabled = true;
        }

		private void SetProgress()
		{
			var currentRound = files.CurrentRound;
			if (currentRound.TotalMatches == 0) { return; }
			progressBar1.Maximum = currentRound.TotalMatches - 1;
			if (currentRound.CurrentMatchIndex != -1)
			{
				progressBar1.Value = currentRound.CurrentMatchIndex;
			}
			label1.Text = $"{roundsCompleted} rounds complete. Select a picture:";
		}

		private void Save(bool exit)
		{
			if (SFDSaveTournament.ShowDialog() == DialogResult.OK)
			{
				string filePath = SFDSaveTournament.FileName;
				File.WriteAllText(filePath, BracketedTournamentSerializer.SerializeString(files));

				if (exit) { Application.Exit(); }
			}
		}

		private void ButtonSaveClose_Click(object sender, EventArgs e)
		{
			Save(true);
		}

		private void ButtonSave_Click(object sender, EventArgs e)
		{
			Save(false);
		}

		private void ButtonUndo_Click(object sender, EventArgs e)
		{
			files.Undo();
			var match = files.CurrentRound.CurrentMatch;
			DisplayPictures(match.Team1, match.Team2);
			SetProgress();
			UpdateLabel();
			ButtonUndo.Enabled = files.CanUndo;
		}

		private void UpdateLabel()
		{
			if (files.CurrentRound == null)
			{
				label2.Text = "Null";
				return;
			}
		
			int currentRoundNumber = files.CurrentRoundNumber;
			int totalMatches = files.CurrentRound.TotalMatches;
			int matchesRemaining = totalMatches - files.CurrentRound.CurrentMatchIndex;

			label2.Text = $"Round {currentRoundNumber} ({matchesRemaining} matches remaining)";
		}
	}
}
