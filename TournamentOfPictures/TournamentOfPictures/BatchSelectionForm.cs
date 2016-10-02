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
	public partial class BatchSelectionForm : Form
	{
		private readonly Color DefaultColor = Color.FromArgb(255, 224, 224, 224);
		private readonly Color FirstColor = Color.FromArgb(255, 184, 201, 219);
		private readonly Color SecondColor = Color.FromArgb(255, 184, 219, 191);
		private readonly Color ThirdColor = Color.FromArgb(219, 216, 184);
		private readonly Color FourthColor = Color.FromArgb(219, 184, 184);

		private BatchSelectionTournament<string>.Batch<string> currentBatch;

		private string folderPath;
		private BatchSelectionTournament<string> tournament;
	
		private int selectionLevel = 3;

		public BatchSelectionForm(string folderPath)
		{
			InitializeComponent();
			this.folderPath = folderPath;
			tournament = new BatchSelectionTournament<string>(
			System.IO.Directory.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly).Where(
			f => (f.ToLower().EndsWith(".png") || f.ToLower().EndsWith(".jpg") || f.ToLower().EndsWith(".jpeg") || f.ToLower().EndsWith(".bmp")
			|| f.ToLower().EndsWith(".gif"))), 4);

			tournament.NewBatchEvent += Tournament_NewBatchEvent;
			tournament.WinnerSelectedEvent += Tournament_WinnerSelectedEvent;
			tournament.Start();
		}

		private void Tournament_NewBatchEvent(BatchSelectionTournament<string>.Batch<string> newBatch)
		{
			currentBatch = newBatch;

			if (newBatch.Items.Count == 4)
			{
				var panels = new[] { PanelTopLeft, PanelTopRight, PanelBottomLeft, PanelBottomRight };
				var items = newBatch.Items.OrderByDescending(i => i.Score).ToList();
				while (items.Count < 4) { items.Add(new ScoredItem<string>(null)); }
				for (int i = 0; i < 4; i++)
				{
					CreatePictureBox(panels[i], items[i].Item);
				}
			}
			else
			{
				int quarterBatch = (int)Math.Ceiling(newBatch.Items.Count / 4f);
				List<List<ScoredItem<string>>> displayBuckets = new List<List<ScoredItem<string>>>(4);
				List<ScoredItem<string>> displayBucket = new List<ScoredItem<string>>(quarterBatch);
				int itemsLeftInBucket = quarterBatch;
				IEnumerator<ScoredItem<string>> batchEnumerator = newBatch.Items.OrderByDescending(i => i.Score).GetEnumerator();
				
				while (batchEnumerator.MoveNext())
				{
					var current = batchEnumerator.Current;
					displayBucket.Add(current);
					itemsLeftInBucket--;

					if (itemsLeftInBucket == 0)
					{
						displayBuckets.Add(displayBucket);
						displayBucket = new List<ScoredItem<string>>(quarterBatch);
					}
				}

				while (displayBuckets.Count < 4)
				{
					displayBuckets.Add(new List<ScoredItem<string>>());
				}

				CreateBatchInfoControls(PanelTopLeft, displayBuckets[0]);
				CreateBatchInfoControls(PanelTopRight, displayBuckets[1]);
				CreateBatchInfoControls(PanelBottomLeft, displayBuckets[2]);
				CreateBatchInfoControls(PanelBottomRight, displayBuckets[3]);
			}
		}

		private void Tournament_WinnerSelectedEvent(string winner, IOrderedEnumerable<string> order)
		{
			throw new NotImplementedException();
		}

		private void Select(Panel panel)
		{
			Color newBackColor = Color.Black;
			switch (selectionLevel)
			{
				case 3:
					newBackColor = FirstColor;
					break;
				case 2:
					newBackColor = SecondColor;
					break;
				case 1:
					newBackColor = ThirdColor;
					break;
				case 0:
					newBackColor = FourthColor;
					break;
				default:
					return;
			}

			panel.BackColor = newBackColor;
			selectionLevel--;
		}

		private void PanelTopLeft_Click(object sender, EventArgs e)
		{
			Select((Panel)sender);
		}

		private void CreatePictureBox(Panel panel, string path)
		{
			PictureBox box = new PictureBox();
			box.SizeMode = PictureBoxSizeMode.Zoom;
			box.Dock = DockStyle.Fill;
			if (path != null) { box.Image = Image.FromFile(path); }
			box.Click += (sender, e) => Select(panel);

			panel.Controls.Clear();
			panel.Controls.Add(box);
		}

		private void CreateBatchInfoControls(Panel panel, IEnumerable<ScoredItem<string>> items)
		{
			panel.Controls.Clear();

			Label label = new Label();
			label.Text = $"Batch of {items.Count()} pictures.";
			label.Location = new Point(4, 4);
			panel.Controls.Add(label);

			Button button = new Button();
			button.Text = "View Pictures...";
			button.Location = new Point(4, 20);
			if (!items.Any()) { button.Enabled = false; }
			button.Click += (sender, e) =>
			{
				var viewer = new ViewerForm();
				viewer.Initialize(items.OrderByDescending(i => i.Score).Select(i => i.Item));
				viewer.ShowDialog();
			};
		}

		private void ButtonStartOver_Click(object sender, EventArgs e)
		{
			selectionLevel = 3;
			PanelTopLeft.BackColor = PanelTopRight.BackColor = PanelBottomLeft.BackColor = PanelBottomRight.BackColor = DefaultBackColor;
		}


	}
}
