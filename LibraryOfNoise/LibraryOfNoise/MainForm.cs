using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryOfNoise
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void NUDSetNumber_ValueChanged(object sender, EventArgs e)
		{
			this.TextPage.Text = NoiseGenerator.GeneratePage((ulong)this.NUDSetNumber.Value, (ulong)this.NUDPageNumber.Value);
		}

		private void NUDPageNumber_ValueChanged(object sender, EventArgs e)
		{
			this.TextPage.Text = NoiseGenerator.GeneratePage((ulong)this.NUDSetNumber.Value, (ulong)this.NUDPageNumber.Value);
		}

		private void ButtonGo_Click(object sender, EventArgs e)
		{
			this.TextPage.Text = NoiseGenerator.GeneratePage((ulong)this.NUDSetNumber.Value, (ulong)this.NUDPageNumber.Value);
		}

		private void ButtonRandomPage_Click(object sender, EventArgs e)
		{
			unchecked
			{
				Random random = new Random();
				uint setHigh = (uint)random.Next(int.MinValue, int.MaxValue);
				uint setLow = (uint)random.Next(int.MinValue, int.MaxValue);

				ulong setNumber = ((ulong)setHigh << 32) + setLow;

				uint pageHigh = (uint)random.Next(int.MinValue, int.MaxValue);
				uint pageLow = (uint)random.Next(int.MinValue, int.MaxValue);

				ulong pageNumber = ((ulong)pageHigh << 32) + pageLow;

				this.NUDSetNumber.Value = setNumber;
				this.NUDPageNumber.Value = pageNumber;
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{

		}

		private void StaticLabelSet_Click(object sender, EventArgs e)
		{

		}

		private void ButtonSearch_Click(object sender, EventArgs e)
		{
			string searchText = this.TextSearch.Text;
			if (string.IsNullOrEmpty(searchText))
			{
				return;
			}

			for (ulong i = 0ul; i < ulong.MaxValue; i++)
			{
				for (ulong j = 0ul; i < ulong.MaxValue; j++)
				{
					if (j % 4096 == 0)
					{
						this.LabelSeed.Text = string.Format("Search: Set {0}, Page {1}", i, j);
						Application.DoEvents(); // EEVVIILL
					}

					string page = NoiseGenerator.GeneratePage(i, j);
					if (page.Contains(searchText))
					{
						this.NUDSetNumber.Value = i;
						this.NUDPageNumber.Value = j;
						return;
					}
				}
			}
		}
	}
}
