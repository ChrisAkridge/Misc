using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TournamentOfPictures
{
	public partial class QuickSortForm : Form
	{
		private List<string> pictures;
		private QuickSorter sorter;

		private int comparisonsMade = 0;
		private int userComparisonsMade = 0;
		private int computerComparisonsMade = 0;

		private AutoResetEvent resetEvent = null;

		public int CompareResult { get; private set; }
		
		public QuickSortForm(string path)
		{
			pictures = GetFilesInFolder(path);
			sorter = new QuickSorter(this, pictures);
			InitializeComponent();
		}

		private List<string> GetFilesInFolder(string folderPath)
		{
			var result = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Where(f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".gif") || f.ToLower().EndsWith(".bmp") || f.ToLower().EndsWith(".jpeg"))).ToList();
			return result;
		}

		private void QuickSortForm_Load(object sender, EventArgs e)
		{
			
		}

		private void UpdateInfoLabel()
		{
			LabelInformation.Text = $"{comparisonsMade} comparisons made. ({userComparisonsMade} user-directed, {computerComparisonsMade} computer-made.)";
		}

		private void DisplayPictures(string picture1, string picture2)
		{
			var oldImage1 = PictureLeft.Image;
			var oldImage2 = PictureRight.Image;

			PictureLeft.Image = ImageLoader.LoadImage(picture1);
			PictureRight.Image = ImageLoader.LoadImage(picture2);

			oldImage1?.Dispose();
			oldImage2?.Dispose();

			if (PictureLeft.Image.Width < PictureLeft.Width && PictureLeft.Image.Height < PictureLeft.Height)
			{
				PictureLeft.SizeMode = PictureBoxSizeMode.CenterImage;
			}
			else
			{
				PictureLeft.SizeMode = PictureBoxSizeMode.Zoom;
			}

			if (PictureRight.Image.Width < PictureRight.Width && PictureRight.Image.Height < PictureRight.Height)
			{
				PictureRight.SizeMode = PictureBoxSizeMode.CenterImage;
			}
			else
			{
				PictureRight.SizeMode = PictureBoxSizeMode.Zoom;
			}
		}

		internal void PrepareComparison(string item1, string item2, AutoResetEvent resetEvent)
		{
			DisplayPictures(item1, item2);
			this.resetEvent = resetEvent;
		}

		private void PictureLeft_Click(object sender, EventArgs e)
		{
			CompareResult = 1;
			resetEvent.Set();
		}

		private void PictureRight_Click(object sender, EventArgs e)
		{
			CompareResult = 2;
			resetEvent.Set();
		}

		private void SortDelayTimer_Tick(object sender, EventArgs e)
		{
			SortDelayTimer.Enabled = false;
			Task.Factory.StartNew(() => sorter.Start());
		}
	}
}
