using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Countdown
{
	public partial class SaveFilePathNotValidForm : Form
	{
		public SaveFilePathNotValidForm()
		{
			InitializeComponent();
		}

		private void SaveFilePathNotValidForm_Load(object sender, EventArgs e)
		{
			TextPath.Text = Properties.Settings.Default.SaveFilePath;
		}

		private void ButtonSelectPath_Click(object sender, EventArgs e)
		{
			if (SFDPath.ShowDialog() == DialogResult.OK)
			{
				TextPath.Text = SFDPath.FileName;
			}
		}

		private void ButtonOK_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.SaveFilePath = TextPath.Text;
			Exception exception;
			if (!IO.SaveFilePathIsValid(out exception))
			{
				string message = $"The selected path is not valid.\r\nReason: {exception?.Message}";
				MessageBox.Show(message, "Countdown", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			Close();
		}
	}
}
