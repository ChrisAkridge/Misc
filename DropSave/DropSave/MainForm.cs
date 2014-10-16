using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DropSave
{
	public partial class MainForm : Form
	{
		private FileSaver saver;

		public MainForm()
		{
			InitializeComponent();

			saver = new FileSaver();
			this.LabelNextFile.Text = string.Format("Next file: {0}", this.saver.PeekNextFile());
		}

		private void MainForm_DragDrop(object sender, DragEventArgs e)
		{
			// See http://stackoverflow.com/questions/13047521/get-file-extension-or-hasextension-type-bool-from-uri-object-c-sharp
			string stringData = e.Data.GetData(typeof(string)) as string;
			Uri fileUri = new Uri(stringData);
			string extension = Path.GetExtension(string.Format("{0}{1}{2}{3}", fileUri.Scheme, Uri.SchemeDelimiter, fileUri.Authority, fileUri.AbsolutePath));

			string newFilePath = this.saver.GetNextFilePath(extension);
			WebClient webClient = new WebClient();
			webClient.DownloadFile(stringData, newFilePath);

			this.LabelNextFile.Text = string.Format("Next file: {0}", this.saver.PeekNextFile());
		}

		private void MainForm_DragEnter(object sender, DragEventArgs e)
		{
			if ((e.AllowedEffect & DragDropEffects.All) != 0 && e.Data.GetDataPresent(typeof(string)))
			{
				e.Effect = DragDropEffects.All;
			}
		}

		private void Notify_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Notify.Visible = true;
				this.Hide();
			}

		}

		private void ButtonUpdate_Click(object sender, EventArgs e)
		{
			saver = new FileSaver();
			this.LabelNextFile.Text = string.Format("Next file: {0}", this.saver.PeekNextFile());
		}

		private void ButtonOpenFolder_Click(object sender, EventArgs e)
		{
			MessageBox.Show("implement this someday");
		}
	}
}
