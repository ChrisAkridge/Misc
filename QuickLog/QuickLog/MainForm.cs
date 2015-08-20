using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickLog
{
	public partial class MainForm : Form
	{
		private const string StepsDataFilePath = @"C:\Users\Chris\Documents\Files\Unclassified Files\Steps.txt";
		private const string SmilesDataFilePath = @"C:\Users\Chris\Documents\Files\Unclassified Files\Smiles.txt";

		private DateTime launchDate;	// guard against setting the wrong date if we wait into the next day

		public MainForm()
		{
			InitializeComponent();

			launchDate = DateTime.Now;
			NUDSteps.Text = "";
			NUDSmiles.Text = "";
		}

		private void ButtonAccept_Click(object sender, EventArgs e)
		{
			try
			{
				string dateString = launchDate.ToString("yyyy-MM-dd");
				string stepsString = $"{dateString},{NUDSteps.Value.ToString()}";
				string smilesString = $"{dateString},{NUDSmiles.Value.ToString()}";

				using (StreamWriter writer = new StreamWriter(StepsDataFilePath, append: true))
				{
					writer.Write(Environment.NewLine);
					writer.Write(stepsString);
				}

				if (NUDSmiles.Value > 0)	// -1 signifies that today was a non-working day
				{
					using (StreamWriter writer = new StreamWriter(SmilesDataFilePath, append: true))
					{
						writer.Write(Environment.NewLine);
						writer.Write(smilesString);
					}
				}
			}
			catch (Exception ex)
			{
				string messageBoxText = $"An error has occurred.\r\n\r\nType:{ex.GetType().Name}\r\n\r\nMessage: {ex.Message}\r\nPlease try again.";
            }
        }
	}
}
