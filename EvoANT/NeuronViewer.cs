using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvoANTCore;

namespace EvoANTFrontend
{
	public partial class NeuronViewer : UserControl
	{
		private const int NeuronDisplayDiameter = 10;
		private const int NeuronDisplaySpacing = 5;

		private Ant ant;
		public Ant Ant
		{
			get { return ant; }
			set
			{
				ant = value;
				CreateDisplayNeurons();
			}
		}
		private Random random = new Random();
		private List<DisplayNeuron> displayNeurons;

		public NeuronViewer(Ant ant)
		{
			Ant = ant;
			InitializeComponent();
			CreateDisplayNeurons();
		}

		private void CreateDisplayNeurons()
		{
			if (Ant == null) { return; }

			displayNeurons = new List<DisplayNeuron>();

			// Lay out the input neurons.
			int x = NeuronDisplayDiameter + NeuronDisplaySpacing;
			int y = NeuronDisplayDiameter + NeuronDisplaySpacing;
			foreach (var neuron in Ant.InputNeurons)
			{
				displayNeurons.Add(new DisplayNeuron(neuron, x, y));
				x += NeuronDisplayDiameter + NeuronDisplaySpacing;
			}

			// Lay out the output neurons.
			x = NeuronDisplayDiameter + NeuronDisplaySpacing;
			y += 250;
			foreach (var neuron in Ant.OutputNeurons)
			{
				displayNeurons.Add(new DisplayNeuron(neuron, x, y));
				x += NeuronDisplayDiameter + NeuronDisplaySpacing;
			}

			// Lay out the hidden neurons.
			x = NeuronDisplayDiameter + NeuronDisplaySpacing;
			y = 35;
			// 20x9 grid size = 180 hidden neurons will fit
			foreach (var neuron in Ant.HiddenLayer.Neurons)
			{
				int drawX = x + random.Next(0, 21) * 15;
				int drawY = y + random.Next(0, 10) * 15;
				displayNeurons.Add(new DisplayNeuron(neuron, drawX + 5, drawY + 5));
			}
		}

		private void NeuronViewer_Paint(object sender, PaintEventArgs e)
		{
			if (Ant == null || Ant.RemainingLifespan <= 0) { return; }

			// Draw the neurons. Fill them in if they're firing.
			Pen neuronCirclePen = new Pen(Color.Black);
			neuronCirclePen.Width = 1f;

			foreach (var displayNeuron in displayNeurons)
			{
				var drawRect = new Rectangle(displayNeuron.CenterX - (NeuronDisplayDiameter / 2),
					displayNeuron.CenterY - (NeuronDisplayDiameter / 2),
					NeuronDisplayDiameter,
					NeuronDisplayDiameter);

				e.Graphics.DrawEllipse(neuronCirclePen, drawRect);

				if (displayNeuron.Neuron.IsFiring)
				{
					e.Graphics.FillEllipse(Brushes.Blue, drawRect);
				}
			}

			// Draw neuronal connections.
			foreach (var displayNeuron in displayNeurons)
			{
				foreach (var outbound in displayNeuron.Neuron.OutboundConnections)
				{
					var connectedNeuron = Lookup(displayNeurons, outbound);
					e.Graphics.DrawLine(neuronCirclePen,
						new Point(displayNeuron.CenterX, displayNeuron.CenterY),
						new Point(connectedNeuron.CenterX, connectedNeuron.CenterY));
				}
			}
		}

		private static DisplayNeuron Lookup(IEnumerable<DisplayNeuron> neurons, Neuron neuron)
		{
			return neurons.First(n => ReferenceEquals(n.Neuron, neuron));
		}
	}

	internal class DisplayNeuron
	{
		public Neuron Neuron { get; }
		public int CenterX { get; }
		public int CenterY { get; }

		public DisplayNeuron(Neuron neuron, int centerX, int centerY)
		{
			Neuron = neuron;
			CenterX = centerX;
			CenterY = centerY;
		}
	}
}
