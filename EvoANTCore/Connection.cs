using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class Connection
	{
		internal ConnectionGene Gene { get; set; }
		public double Weight { get; internal set; }

		public Neuron From { get; internal set; }
		public Neuron To { get; internal set; }

		public void TransmitSignal(double signalStrength)
		{
			signalStrength *= Weight;
			To.InboundNeuronFired(signalStrength);
		}
	}
}
