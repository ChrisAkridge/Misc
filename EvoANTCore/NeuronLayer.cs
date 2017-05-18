using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class NeuronLayer
	{
		private static Random random = new Random();

		public List<Neuron> Neurons { get; private set; } = new List<Neuron>();

		private static NeuronLayer CreateRandom(WorldSettings settings)
		{
			var layer = new NeuronLayer();

			int neuronsToCreate = random.Next(1, settings.MaxRandomNeuronsPerNewAnt + 1);
			for (int i = 0; i < neuronsToCreate; i++)
			{
				var neuron = Neuron.CreateRandom(settings);
				layer.Neurons.Add(neuron);
			}

			if (neuronsToCreate == 1) { return layer; }

			for (int i = 0; i < layer.Neurons.Count; i++)
			{
				// Create random connections between neurons
				int numberOfConnections = random.Next(1, settings.MaxRandomConnectionsBetweenNeurons + 1);
				for (int j = 0; j < numberOfConnections; j++)
				{
					int neuronToConnectToIndex = random.Next(0, layer.Neurons.Count);
					while (neuronToConnectToIndex == i)
					{
						neuronToConnectToIndex = random.Next(0, layer.Neurons.Count);
					}

					var sourceNeuron = layer.Neurons[i];
					var destinationNeuron = layer.Neurons[neuronToConnectToIndex];
					sourceNeuron.OutboundConnections.Add(destinationNeuron);
				}
			}

			return layer;
		}

		// method to hook up input neurons
		private static void WireInputs(NeuronLayer layer, IEnumerable<Neuron> inputNeurons,
			WorldSettings settings)
		{
			int layerNeuronCount = layer.Neurons.Count;

			foreach (var input in inputNeurons)
			{
				int numberOfConnections = random.Next(1, settings.MaxRandomConnectionsBetweenNeurons + 1);
				for (int i = 0; i < numberOfConnections; i++)
				{
					int neuronToConnectToIndex = random.Next(0, layerNeuronCount);
					var destinationNeuron = layer.Neurons[neuronToConnectToIndex];
					input.OutboundConnections.Add(destinationNeuron);
				}
			}
		}

		// method to hook up output neurons
		private static void WireOutputs(NeuronLayer layer, IEnumerable<Neuron> outputNeurons,
			WorldSettings settings)
		{
			int layerNeuronCount = layer.Neurons.Count;
			var outputNeuronList = outputNeurons.ToList();

			foreach (var neuron in layer.Neurons)
			{
				int numberOfConnections = random.Next(1, settings.MaxRandomConnectionsBetweenNeurons + 1);
				for (int i = 0; i < numberOfConnections; i++)
				{
					int neuronToConnectToIndex = random.Next(0, outputNeuronList.Count);
					var destinationNeuron = outputNeuronList[neuronToConnectToIndex];
					neuron.OutboundConnections.Add(destinationNeuron);
				}
			}
		}

		public static NeuronLayer CreateRandomAndConnect(IEnumerable<Neuron> inputNeurons,
			IEnumerable<Neuron> outputNeurons, WorldSettings settings)
		{
			var layer = CreateRandom(settings);
			WireInputs(layer, inputNeurons, settings);
			WireOutputs(layer, outputNeurons, settings);
			return layer;
		}

		// Breeding Functions
		private NeuronLayer SeparateHalfOfNeuronsRandomly()
		{
			// Select the neurons to separate.
			// We can shuffle the list of neurons here, because if we're here, we won't be needing
			// this layer anymore after breeding.
			int halfNeurons = Neurons.Count / 2;

			// https://gist.github.com/mikedugan/8249637
			for (int i = Neurons.Count; i > 1; i--)
			{
				int j = random.Next(i);
				Neuron temp = Neurons[j];
				Neurons[j] = Neurons[i - 1];
				Neurons[i - 1] = temp;
			}

			var neuronsToSeparate = Neurons.Take(halfNeurons);
			var newLayer = new NeuronLayer();
			newLayer.Neurons = neuronsToSeparate.ToList();

			// Sever any connections to neurons not in the separated neurons except for output neurons.
			// We'll hook those back up later.
			foreach (var neuron in newLayer.Neurons)
			{
				neuron.OutboundConnections.RemoveAll(n => !newLayer.Neurons.Contains(n) && !n.IsOutputNeuron);
			}

			return newLayer;
		}

		private static NeuronLayer MergeLayersAndGenerateNewConnections(NeuronLayer a, 
			NeuronLayer b, WorldSettings settings)
		{
			var layer = new NeuronLayer();
			layer.Neurons.AddRange(a.Neurons);
			layer.Neurons.AddRange(b.Neurons);

			// Add new random connections.
			for (int i = 0; i < layer.Neurons.Count; i++)
			{
				// Create random connections between neurons
				int numberOfConnections = random.Next(1, settings.MaxRandomConnectionsBetweenNeurons + 1);
				for (int j = 0; j < numberOfConnections; j++)
				{
					int neuronToConnectToIndex = random.Next(0, layer.Neurons.Count);
					while (neuronToConnectToIndex == i)
					{
						neuronToConnectToIndex = random.Next(0, layer.Neurons.Count);
					}

					var sourceNeuron = layer.Neurons[i];
					var destinationNeuron = layer.Neurons[neuronToConnectToIndex];
					sourceNeuron.OutboundConnections.Add(destinationNeuron);
				}
			}

			return layer;
		}

		public static NeuronLayer MergeHiddenLayers(NeuronLayer a, NeuronLayer b, WorldSettings settings)
		{
			var layerA = a.SeparateHalfOfNeuronsRandomly();
			var layerB = b.SeparateHalfOfNeuronsRandomly();

			return MergeLayersAndGenerateNewConnections(a, b, settings);
		}
	}
}