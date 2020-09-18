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

namespace TournamentOfPictures
{
	public partial class PictureRatingSelector : Form
	{
		private bool isRecursive;
		private string folderPath;
		private RatingSelector<string> ratingSelector;
		private RecursiveRatingSelector<string> recursiveSelector;
		
		public PictureRatingSelector(string folderPath, bool isRecursive)
		{
			InitializeComponent();

			this.isRecursive = isRecursive;

			var files = GetFilesInFolder(folderPath);
			if (!isRecursive)
			{
				ratingSelector = new RatingSelector<string>(files);

				ratingSelector.CurrentChangedEvent += RatingSelector_CurrentChangedEvent;
				ratingSelector.AllItemsRatedEvent += RatingSelector_AllItemsRatedEvent;
			}
			else
			{
				recursiveSelector = new RecursiveRatingSelector<string>(files);

				recursiveSelector.CurrentChangedEvent += RecursiveSelector_CurrentChangedEvent;
				recursiveSelector.AllItemsRatedEvent += RecursiveSelector_AllItemsRatedEvent;
				recursiveSelector.NewScoreGroupEvent += RecursiveSelector_NewScoreGroupEvent;
			}
		}

		private void RecursiveSelector_NewScoreGroupEvent(object sender, int e)
		{
			LabelCurrentScoreGroup.Text = $"Group: {e}, Count: {recursiveSelector.CurrentGroupItemCount}";
        }

		private void RecursiveSelector_AllItemsRatedEvent(object sender, List<string> e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Tournament of Pictures";
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				string path = sfd.FileName;

				StringBuilder builder = new StringBuilder();
				foreach (string item in e)
				{
					builder.AppendLine(item);
				}
				File.WriteAllText(path, builder.ToString());
			}
			Close();
		}

		private void RecursiveSelector_CurrentChangedEvent(object sender, string e)
		{
			LoadPicture(e);
		}

		private void RatingSelector_CurrentChangedEvent(object sender, string e)
		{
			LoadPicture(e);
		}

		private void RatingSelector_AllItemsRatedEvent(object sender, List<string> e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Tournament of Pictures";
			if (sfd.ShowDialog() == DialogResult.OK)
			{
				string path = sfd.FileName;

				StringBuilder builder = new StringBuilder();
				foreach (string item in e)
				{
					builder.AppendLine(item);
				}
				File.WriteAllText(path, builder.ToString());
			}
			Close();
		}

		private List<string> GetFilesInFolder(string folderPath)
		{
			var result = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).Where(f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".gif") || f.ToLower().EndsWith(".bmp") || f.ToLower().EndsWith(".jpeg"))).ToList();
			return result;
		}

		private void PictureRatingSelector_Load(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				LoadPicture(ratingSelector.Current);
			}
			else
			{
				LoadPicture(recursiveSelector.Current);
			}
		}

		private void LoadPicture(string picturePath)
		{
			Picture.Image = ImageLoader.LoadImage(picturePath);
		}

		private void ButtonRate1_Click(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				ratingSelector.RateCurrent(1);
			}
			else
			{
				recursiveSelector.RateCurrent(1);
			}
		}

		private void ButtonRate2_Click(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				ratingSelector.RateCurrent(2);
			}
			else
			{
				recursiveSelector.RateCurrent(2);
			}
		}

		private void ButtonRate3_Click(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				ratingSelector.RateCurrent(3);
			}
			else
			{
				recursiveSelector.RateCurrent(3);
			}
		}

		private void ButtonRate4_Click(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				ratingSelector.RateCurrent(4);
			}
			else
			{
				recursiveSelector.RateCurrent(4);
			}
		}

		private void ButtonRate5_Click(object sender, EventArgs e)
		{
			if (!isRecursive)
			{
				ratingSelector.RateCurrent(5);
			}
			else
			{
				recursiveSelector.RateCurrent(5);
			}
		}
	}
}
