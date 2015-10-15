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
            this.filePath = path;
			this.standings = standings;
            this.pictureBox1.Image = Image.FromFile(path);
            if (this.pictureBox1.Image.Width < this.pictureBox1.Width && this.pictureBox1.Image.Height < this.pictureBox1.Height)
            {
                this.pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            }
            else
            {
                this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            this.label2.Text = string.Format("File at {0} (click to open)", path);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Process process = new Process();
            process.StartInfo.FileName = "C:\\Program Files (x86)\\IrfanView\\i_view32.exe";
            process.StartInfo.Arguments = this.filePath;
            process.Start();
        }

		private void LLbViewStandings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			new StandingsForm(this.standings).ShowDialog();
		}
    }
}
