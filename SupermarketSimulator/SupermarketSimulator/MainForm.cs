using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SupermarketSimulator
{
	public partial class MainForm : Form
	{
		private Game game;
		private bool within = false;

		public MainForm()
		{
			InitializeComponent();
		}

		private void ListBoxBuyingOptions_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!within)
			{
				within = true;

				within = false;
			}
		}

		private void ConstructData()
		{

		}

		private void timer1_Tick(object sender, EventArgs e)
		{
		}
	}
}
