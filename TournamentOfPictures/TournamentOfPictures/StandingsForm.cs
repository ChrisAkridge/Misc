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
		public StandingsForm(string standings)
		{
			InitializeComponent();

			TextStandings.Text = standings;
		}

		private void StandingsForm_Load(object sender, EventArgs e)
		{

		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
