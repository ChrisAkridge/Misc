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
				Panel[] panels = new[] { PanelTopLeft, PanelTopRight, PanelBottomLeft, PanelBottomRight };
				List<ScoredItem<string>> items = newBatch.Items.OrderByDescending(i => i.Score).ToList();
				while (items.Count < 4) { items.Add(new ScoredItem<string>(null)); }
				for (int i = 0; i < 4; i++)
				{
					CreatePictureBox(panels[i], items[i].Item);
				}
			}
			else
			{
				int quarterBatch = (int)Math.Ceiling(newBatch.Items.Count / 4f);
				var displayBuckets = new List<List<ScoredItem<string>>>(4);
				var displayBucket = new List<ScoredItem<string>>(quarterBatch);
				int itemsLeftInBucket = quarterBatch;
				IEnumerator<ScoredItem<string>> batchEnumerator =
					newBatch.Items.OrderByDescending(i => i.Score).GetEnumerator();
				
				while (batchEnumerator.MoveNext())
				{
					ScoredItem<string> current = batchEnumerator.Current;
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

				batchEnumerator.Dispose();
			}
		}

		private static void Tournament_WinnerSelectedEvent(string winner, IOrderedEnumerable<string> order)
		{
			throw new NotImplementedException();
		}

		private void Select(Control control)
		{
			Color newBackColor;
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

			control.BackColor = newBackColor;
			selectionLevel--;
		}

		private void PanelTopLeft_Click(object sender, EventArgs e)
		{
			Select((Panel)sender);
		}

		private void CreatePictureBox(Control control, string path)
		{
			var box = new PictureBox
			{
				SizeMode = PictureBoxSizeMode.Zoom,
				Dock = DockStyle.Fill
			};

			if (path != null) { box.Image = Image.FromFile(path); }
			box.Click += (sender, e) => Select(control);

			control.Controls.Clear();
			control.Controls.Add(box);
		}

		private static void CreateBatchInfoControls(Control control, IEnumerable<ScoredItem<string>> items)
		{
			control.Controls.Clear();

			var label = new Label
			{
				Text = $"Batch of {items.Count()} pictures.",
				Location = new Point(4, 4)
			};
			control.Controls.Add(label);

			Button button = new Button
			{
				Text = "View Pictures...",
				Location = new Point(4, 20)
			};

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
