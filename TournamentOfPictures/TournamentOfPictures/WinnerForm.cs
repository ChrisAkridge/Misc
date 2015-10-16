using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TournamentOfPictures
{
    public partial class WinnerForm : Form
    {
        private string filePath;
		private string standings;
        public WinnerForm()
        {
            InitializeComponent();
        }

        public void ShowWinner(string path, string standings)
        {
			filePath = path;
			this.standings = standings;
			pictureBox1.Image = Image.FromFile(path);
            if (pictureBox1.Image.Width < pictureBox1.Width && pictureBox1.Image.Height < pictureBox1.Height)
            {
				pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
				pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
			label2.Text = string.Format("File at {0} (click to open)", path);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "C:\\Program Files (x86)\\IrfanView\\i_view32.exe";
            process.StartInfo.Arguments = filePath;
            process.Start();
        }

		private void LLbViewStandings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			new StandingsForm(standings).ShowDialog();
		}
    }
}
