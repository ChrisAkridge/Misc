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
	public partial class StandingsForm : Form
	{
		private List<ScoredItem<string>> standings;	

		public StandingsForm(IEnumerable<ScoredItem<string>> standings)
		{
			InitializeComponent();

			TextStandings.Text = string.Join(Environment.NewLine, standings.Select(i => i.ToString()));
			this.standings = standings.ToList();
		}

		private void StandingsForm_Load(object sender, EventArgs e)
		{

		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void ButtonSavePlaylist_Click(object sender, EventArgs e)
		{
			if (SFDSavePlaylist.ShowDialog() == DialogResult.OK)
			{
				string savePath = SFDSavePlaylist.FileName;
				System.IO.File.WriteAllLines(savePath, standings.Select(i => i.Item).ToArray());
			}
		}
	}
}
