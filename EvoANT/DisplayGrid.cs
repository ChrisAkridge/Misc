using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvoANTCore;
using System.Drawing.Design;

namespace EvoANTFrontend
{
	public partial class DisplayGrid : UserControl
	{
		private const int GridCellEdgeLength = 32;

		public World World { get; set; }
		private List<NeuronViewerForm> neuronViewerForms = new List<NeuronViewerForm>();

		private Image wallImage;
		private Image foodImage;
		private Image pheromoneImage;
		private Image antImage;

		[Description("The color of the grid lines.")]
		[Category("Appearance")]
		[Editor(typeof(Color), typeof(UITypeEditor))]
		public Color GridLineColor { get; set; } = Color.Black;

		public DisplayGrid()
		{
			InitializeComponent();

			// http://stackoverflow.com/a/8074084/2709212

			LoadImages();
		}

		private void LoadImages()
		{
			wallImage = Image.FromFile(Path.Combine("graphics", "wall.jpg"));
			foodImage = Image.FromFile(Path.Combine("graphics", "food.png"));
			pheromoneImage = Image.FromFile(Path.Combine("graphics", "pheromone.png"));
			antImage = Image.FromFile(Path.Combine("graphics", "ant.png"));
		}

		private void DisplayGrid_Paint(object sender, PaintEventArgs e)
		{
			if (World == null) { return; }

			var graphics = e.Graphics;
			Pen gridLinePen = new Pen(GridLineColor, 1f);

			int renderWidth = World.Width * GridCellEdgeLength;
			int renderHeight = World.Height * GridCellEdgeLength;

			// Draw each vertical line for each grid cell, one pixel wide, on the leftmost pixel
			// of each 64-pixel span.
			for (int i = 0; i <= World.Width; i++)
			{
				int renderPosition = i * GridCellEdgeLength;
				graphics.DrawLine(gridLinePen, new Point(renderPosition, 0),
					new Point(renderPosition, renderHeight));
			}

			// Next, draw each horizontal line in the same fashion.
			for (int i = 0; i <= World.Height; i++)
			{
				int renderPosition = i * GridCellEdgeLength;
				graphics.DrawLine(gridLinePen, new Point(0, renderPosition),
					new Point(renderWidth, renderPosition));
			}

			// Draw each world object in its cell.
			foreach (var item in World.Objects)
			{
				var drawRect = new Rectangle(item.PositionX * GridCellEdgeLength,
					item.PositionY * GridCellEdgeLength, GridCellEdgeLength, GridCellEdgeLength);
				var antLifeFont = new Font("Consolas", 7f);
				var antLifeBrush = new SolidBrush(Color.Black);

				if (item is Food)
				{
					graphics.DrawImage(foodImage, drawRect);
				}
				else if (item is Wall)
				{
					graphics.DrawImage(wallImage, drawRect);
				}
				else if (item is Pheromone)
				{
					graphics.FillRectangle(Brushes.Purple, new Rectangle(drawRect.X, drawRect.Y, 4, 4));
				}
				else if (item is Ant)
				{
					Ant ant = item as Ant;
					if (ant.RemainingLifespan <= 0) { continue; }
					graphics.DrawImage(antImage, drawRect);
					graphics.DrawString(ant.RemainingLifespan.ToString(), antLifeFont, antLifeBrush,
						new PointF(drawRect.X + 2f, drawRect.Y + 2f));
				}
			}

			// Update any neuron viewer forms that happen to be open.
			foreach (var form in neuronViewerForms)
			{
				form.Refresh();
			}
		}

		private void DisplayGrid_Click(object sender, EventArgs e)
		{
			var mousePosition = Cursor.Position;
			var clickPosition = PointToClient(mousePosition);

			int xCell = clickPosition.X / GridCellEdgeLength;
			int yCell = clickPosition.Y / GridCellEdgeLength;

			foreach (var obj in World.GetObjectsAtPosition(xCell, yCell))
			{
				if (obj is Ant)
				{
					var neuronViewerForm = new NeuronViewerForm((Ant)obj);
					neuronViewerForms.Add(neuronViewerForm);
					neuronViewerForm.FormClosed += (s, ev) => neuronViewerForms.Remove(neuronViewerForm);
					neuronViewerForm.Show();
				}
			}
		}
	}
}
