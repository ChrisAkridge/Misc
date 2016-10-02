using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public partial class ViewerForm : Form
	{
		private PictureList list;

		public ViewerForm()
		{
			InitializeComponent();
		}

		public void Initialize(IEnumerable<string> filePaths)
		{
			list = new PictureList(filePaths);
			TrackImagePosition.Maximum = list.Count;
			LoadImage(0);
		}

		private void LoadImage(int index)
		{
			string path = list[index];
			Picture.Image = Image.FromFile(path);
			LabelInfo.Text = GetInfo(index);
		}

		public string GetInfo(int index)
		{
			return $"File {index + 1} of {list.Count}\r\nFilename:{System.IO.Path.GetFileName(list[index])}";
		}

		private void TrackImagePosition_Scroll(object sender, EventArgs e)
		{
			LoadImage(TrackImagePosition.Value - 1);
		}
	}
}
