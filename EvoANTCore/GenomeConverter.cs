using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal static class GenomeConverter
	{
		public static NeuralNetwork ConvertGenomeToNetwork(Genome genome)
		{
			var neurons = new List<Neuron>();
			var connections = new List<Connection>();

			foreach (var gene in genome.Genes)
			{
				if (gene is NeuronGene) { neurons.Add(ConvertGeneToNeuron((NeuronGene)gene)); }
				else if (gene is ConnectionGene)
				{
					connections.Add(ConvertGeneToConnection((ConnectionGene)gene));
				}
			}

			// Wire up each connection.
			foreach (var connection in connections)
			{
				var fromNeuron = LookupNeuronByIndex(neurons, connection.Gene.FromIndex);
				var toNeuron = LookupNeuronByIndex(neurons, connection.Gene.ToIndex);
				connection.From = fromNeuron;
				connection.To = toNeuron;
				fromNeuron.AddOutboundConnection(connection);
			}

			return new NeuralNetwork(neurons);
		}

		private static Neuron ConvertGeneToNeuron(NeuronGene gene)
		{
			var neuron = new Neuron();
			neuron.Gene = gene;
			return neuron;
		}

		private static Connection ConvertGeneToConnection(ConnectionGene gene)
		{
			var connection = new Connection();
			connection.Gene = gene;
			return connection;
		}

		private static Neuron LookupNeuronByIndex(IEnumerable<Neuron> neurons, int index)
		{
			return neurons.First(n => n.Gene.Index == index);
		}
	}
}
