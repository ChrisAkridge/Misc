using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal sealed class Genome
	{
		private const double Coefficient1 = 1d;
		private const double Coefficient2 = 1d;
		private const double Coefficient3 = 0.4d;

		private List<IGene> genes = new List<IGene>();

		private int InnovationNumber { get; set; }
		private int HighestNeuronIndex { get; set; }
		public int GlobalRank { get; internal set; }
		public double Fitness { get; internal set; }
		public IReadOnlyList<IGene> Genes => genes.AsReadOnly();

		public void AddGene(IGene gene)
		{
			InnovationNumber++;
			gene.InnovationNumber = InnovationNumber;
			genes.Add(gene);

			if (gene is NeuronGene)
			{
				var neuron = gene as NeuronGene;
				if (neuron.Index < HighestNeuronIndex)
				{
					HighestNeuronIndex++;
					neuron.Index = HighestNeuronIndex;
				}
			}
		}

		public IGene[] InnovationIndexedGenes()
		{
			var mostInnovatedGene = genes.OrderBy(g => g.InnovationNumber).Last();
			var result = new IGene[mostInnovatedGene.InnovationNumber];

			foreach (var gene in genes)
			{
				result[gene.InnovationNumber] = gene;
			}

			return result;
		}

		public GeneComparisonType CompareGenes(Genome other, int innovationNumber)
		{
			int thisHighestInnovation = InnovationNumber;
			int otherHighestInnovation = other.InnovationNumber;

			var thisGene = genes.FirstOrDefault(g => g.InnovationNumber == innovationNumber);
			var otherGene = other.Genes.FirstOrDefault(g => g.InnovationNumber == innovationNumber);

			if (thisGene == null) 
			{
				throw new ArgumentException($"This genome does not have a gene with innovation number {innovationNumber}");
			}
			else if (otherGene == null && innovationNumber < otherHighestInnovation)
			{
				return GeneComparisonType.Disjoint;
			}
			else if (otherGene == null && innovationNumber > otherHighestInnovation)
			{
				return GeneComparisonType.Excess;
			}
			else { return GeneComparisonType.Matched; }
		}

		private double GetAverageWeightDifferences(Genome other)
		{
			double sum = 0d;
			int matchingGeneCount = 0;

			var thisGenes = InnovationIndexedGenes();
			var otherGenes = other.InnovationIndexedGenes();

			for (int i = 0; i < thisGenes.Length; i++)
			{
				var g1 = thisGenes[i];
				var g2 = otherGenes[i];

				if (g1 == null || g2 == null) { continue; }
				if (g1 is NeuronGene || g2 is NeuronGene) { continue; }

				sum += ((ConnectionGene)g2).Weight - ((ConnectionGene)g1).Weight;
				matchingGeneCount++;
			}

			return sum / matchingGeneCount;
		}

		public double GetCompatibilityDistance(Genome other)
		{
			int disjointCount = 0;
			int excessCount = 0;

			foreach (var gene in genes)
			{
				var comparison = CompareGenes(other, gene.InnovationNumber);
				if (comparison == GeneComparisonType.Disjoint) { disjointCount++; }
				else if (comparison == GeneComparisonType.Excess) { excessCount++; }
			}

			double averageWeightDifferences = GetAverageWeightDifferences(other);
			double largerGenomeSize = (genes.Count > other.genes.Count) ? genes.Count : other.Genes.Count;

			double _1 = (Coefficient1 * excessCount) / largerGenomeSize;
			double _2 = (Coefficient2 * disjointCount) / largerGenomeSize;
			return _1 + _2 + (Coefficient3 * averageWeightDifferences);
		}

		public static Genome CreateRandom()
		{
			var genome = new Genome();
			Reproduction.AddInputOutputNeurons(genome);

			// Generate random neurons for the hidden layer.
			int neuronsToGenerate = GlobalRandom.Next(11);
			for (int i = 0; i < neuronsToGenerate; i++)
			{
				var neuronGene = Reproduction.CreateRandomNeuron();
				genome.AddGene(neuronGene);
			}

			// Generate random connections between neurons.
			int connectionsToGenerate = GlobalRandom.Next(31);
			List<ConnectionGene> connections = new List<ConnectionGene>(connectionsToGenerate);
			for (int i = 0; i < connectionsToGenerate; i++)
			{
				var neuronPair = GlobalRandom.ChooseRandomPair(genome.genes);
				var connection = new ConnectionGene();
				connection.FromIndex = ((NeuronGene)neuronPair.Item1).Index;
				connection.ToIndex = ((NeuronGene)neuronPair.Item2).Index;
				connection.Weight = GlobalRandom.NextDouble();
				connections.Add(connection);
			}

			foreach (var connection in connections) { genome.AddGene(connection); }

			return genome;
		}

		public static Genome Crossover(Genome child, Genome g1, Genome g2)
		{
			if (g2.Fitness > g1.Fitness)
			{
				var gTemp = g1;
				g1 = g2;
				g2 = gTemp;
			}

			var g2Genes = g2.InnovationIndexedGenes();

			foreach (var g1Gene in g1.Genes)
			{
				var matchingGene = g2Genes.FirstOrDefault(g => g.InnovationNumber == g1Gene.InnovationNumber);
				if (matchingGene != null && GlobalRandom.Next(2) == 1)
				{
					child.AddGene(matchingGene.Clone());
				}
				else { child.AddGene(g1Gene.Clone()); }
			}

			return child;
		}

		public static Genome DirectClone(Genome child, Genome parent)
		{
			foreach (var gene in parent.Genes)
			{
				child.AddGene(gene.Clone());
			}

			return child;
		}

		public void Mutate()
		{
			var mutationType = (MutationType)GlobalRandom.Next(0, 4);

			switch (mutationType)
			{
				case MutationType.AddConnection:
					MutateAddConnection(); break;
				case MutationType.AddNeuron:
					MutateAddNeuron(); break;
				case MutationType.RemoveConnection:
					MutateRemoveConnection(); break;
				case MutationType.RemoveNeuron:
					MutateRemoveNeuron(); break;
				default:
					throw new ArgumentException($"Generated invalid mutation type {(int)mutationType}.");
			}
		}

		private void MutateAddConnection()
		{
			var acceptableNeurons = genes.Where(g => g is NeuronGene).Cast<NeuronGene>()
					.Where(n => n.Type == NeuronType.Input || n.Type == NeuronType.Hidden).ToList();
			var neurons = GlobalRandom.ChooseRandomPair(acceptableNeurons);
			var neuronA = neurons.Item1;
			var neuronB = neurons.Item2;

			var connection = new ConnectionGene();
			connection.FromIndex = neuronA.Index;
			connection.ToIndex = neuronB.Index;
			connection.Weight = 1d;
			AddGene(connection);
		}

		private void MutateAddNeuron()
		{
			var connectionToSplit = (ConnectionGene)GlobalRandom.ChooseRandom(genes.Where(g => g is ConnectionGene));
			int oldFromIndex = connectionToSplit.FromIndex;
			int oldToIndex = connectionToSplit.ToIndex;
			double oldWeight = connectionToSplit.Weight;

			var newNeuron = Reproduction.CreateRandomNeuron();
			AddGene(newNeuron);

			var connectionFromToNew = new ConnectionGene();
			connectionFromToNew.FromIndex = oldFromIndex;
			connectionFromToNew.ToIndex = newNeuron.Index;
			connectionFromToNew.Weight = oldWeight;

			var connectionNewToTo = new ConnectionGene();
			connectionNewToTo.FromIndex = newNeuron.Index;
			connectionNewToTo.ToIndex = oldToIndex;
			connectionNewToTo.Weight = oldWeight;

			AddGene(connectionFromToNew);
			AddGene(connectionNewToTo);
		}

		private void MutateRemoveConnection()
		{
			var connection = GlobalRandom.ChooseRandom(genes.Where(g => g is ConnectionGene));
			genes.Remove(connection);
		}

		private void MutateRemoveNeuron()
		{
			var neuron = (NeuronGene)GlobalRandom.ChooseRandom(genes.Where(g => g is NeuronGene));
			var allConnections = genes.Where(g => g is ConnectionGene).Cast<ConnectionGene>();
			var inboundConnections = allConnections.Where(c => c.ToIndex == neuron.Index);
			var outboundConnections = allConnections.Where(c => c.FromIndex == neuron.Index);
			var connectionsToRemove = inboundConnections.Concat(outboundConnections).ToList();

			genes.RemoveAll(g => connectionsToRemove.Contains(g));
		}
	}
}
