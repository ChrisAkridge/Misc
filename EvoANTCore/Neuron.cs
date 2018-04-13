using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class Neuron
	{
		private List<Connection> outboundConnections = new List<Connection>();
		private double inboundSignalStrength;

		internal NeuronGene Gene { get; set; }
		public IReadOnlyList<Connection> OutboundConnections => outboundConnections.AsReadOnly();
		public bool IsFiring { get; set; }

		public double CurrentFiringRequirement { get; internal set; }

		public void AddOutboundConnection(Connection connection) => outboundConnections.Add(connection);

		public void ResetForNextTimestep()
		{
			inboundSignalStrength = 0d;
			IsFiring = false;

			CurrentFiringRequirement -= Gene.FatigueDecrease;
			if (CurrentFiringRequirement < Gene.FiringRequirement)
			{
				CurrentFiringRequirement = Gene.FiringRequirement;
			}
		}

		public void InboundNeuronFired(double signalStrength)
		{
			inboundSignalStrength += signalStrength;
			if (inboundSignalStrength >= CurrentFiringRequirement)
			{
				Fire();
			}
		}

		public void Fire()
		{
			IsFiring = true;

			foreach (var connection in outboundConnections)
			{
				connection.TransmitSignal(Gene.FiringStrength);
			}

			CurrentFiringRequirement += Gene.FatigueIncrease;
			Gene.OnFire?.Invoke();
		}
	}
}
