using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class NeuralNetwork
	{
		private List<Neuron> neurons = new List<Neuron>();

		internal Genome Genome { get; set; }
		public IReadOnlyList<Neuron> Neurons => neurons.AsReadOnly();
		public double Fitness { get; set; }

		public NeuralNetwork(IEnumerable<Neuron> neurons)
		{
			this.neurons.AddRange(neurons);
		}

		public Neuron LookupBySpecialName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("The name of the neuron was null or empty.");
			}

			var matchingNeurons = neurons.Where(n => n.Gene.SpecialName == name);
			int count = matchingNeurons.Count();

			if (count == 0)
			{
				throw new ArgumentException($"No neuron in the network is named {name}.");
			}
			else if (count > 1)
			{
				throw new ArgumentException($"Multiple neurons ({count}, to be exact) have the same name {name}.");
			}

			return matchingNeurons.First();
		}
	}
}
