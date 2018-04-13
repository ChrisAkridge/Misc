using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal static class Reproduction
	{
		public static NeuronGene CreateRandomNeuron()
		{
			var neuron = new NeuronGene();

			// TODO: figure out world settings (global? constant? singleton?)
			neuron.Type = NeuronType.Hidden;
			neuron.FiringStrength = GlobalRandom.NextDouble() * 10d;
			neuron.FiringRequirement = GlobalRandom.NextDouble() * 20d;
			neuron.FatigueIncrease = GlobalRandom.NextDouble() * 1d;
			neuron.FatigueDecrease = GlobalRandom.NextDouble() * 0.8d;

			return neuron;
		}

		public static void AddInputOutputNeurons(Genome genome)
		{
			var inputFoodPresent = new NeuronGene();
			var inputWallAhead = new NeuronGene();
			var inputWallBehind = new NeuronGene();
			var inputWallOnLeft = new NeuronGene();
			var inputWallOnRight = new NeuronGene();
			var inputPheromonePresent = new NeuronGene();
			var inputHunger = new NeuronGene();

			inputFoodPresent.Index = 0;
			inputFoodPresent.SpecialName = "inFoodPresent";

			inputWallAhead.Index = 1;
			inputWallAhead.SpecialName = "inWallAhead";

			inputWallBehind.Index = 2;
			inputWallBehind.SpecialName = "inWallBehind";

			inputWallOnLeft.Index = 3;
			inputWallOnLeft.SpecialName = "inWallOnLeft";

			inputWallOnRight.Index = 4;
			inputWallOnRight.SpecialName = "inWallOnRight";

			inputPheromonePresent.Index = 5;
			inputPheromonePresent.SpecialName = "inPheromonePresent";

			inputHunger.Index = 6;
			inputHunger.SpecialName = "inHunger";

			inputFoodPresent.FiringStrength = inputWallAhead.FiringStrength = inputWallBehind.FiringStrength =
				inputWallOnLeft.FiringStrength = inputWallOnRight.FiringStrength = inputPheromonePresent.FiringStrength =
				inputHunger.FiringStrength = 10d;

			inputFoodPresent.Type = inputWallAhead.Type = inputWallBehind.Type =
				inputWallOnLeft.Type = inputWallOnRight.Type = inputPheromonePresent.Type =
				inputHunger.Type = NeuronType.Input;

			var outputEat = new NeuronGene();
			var outputMoveForward = new NeuronGene();
			var outputMoveBackward = new NeuronGene();
			var outputMoveLeft = new NeuronGene();
			var outputMoveRight = new NeuronGene();
			var outputPlacePheromone = new NeuronGene();

			outputEat.Index = 7;
			outputEat.SpecialName = "outEat";

			outputMoveForward.Index = 8;
			outputMoveForward.SpecialName = "outMoveForward";

			outputMoveBackward.Index = 9;
			outputMoveBackward.SpecialName = "outMoveBackward";

			outputMoveLeft.Index = 10;
			outputMoveLeft.SpecialName = "outMoveLeft";

			outputMoveRight.Index = 11;
			outputMoveRight.SpecialName = "outMoveRight";

			outputPlacePheromone.Index = 12;
			outputPlacePheromone.SpecialName = "outPlacePheromone";

			outputEat.FatigueIncrease = outputMoveForward.FatigueIncrease =
				outputMoveBackward.FatigueIncrease = outputMoveLeft.FatigueIncrease =
				outputMoveRight.FatigueIncrease = outputPlacePheromone.FatigueIncrease = 1d;

			outputEat.FatigueDecrease = outputMoveForward.FatigueDecrease =
				outputMoveBackward.FatigueDecrease = outputMoveLeft.FatigueDecrease =
				outputMoveRight.FatigueDecrease = outputPlacePheromone.FatigueDecrease = 0.8d;

			genome.AddGene(inputFoodPresent);
			genome.AddGene(inputWallAhead);
			genome.AddGene(inputWallBehind);
			genome.AddGene(inputWallOnLeft);
			genome.AddGene(inputWallOnRight);
			genome.AddGene(inputPheromonePresent);
			genome.AddGene(inputHunger);
			genome.AddGene(outputEat);
			genome.AddGene(outputMoveForward);
			genome.AddGene(outputMoveBackward);
			genome.AddGene(outputMoveLeft);
			genome.AddGene(outputMoveRight);
			genome.AddGene(outputPlacePheromone);
		}

		public static void CreateRandomGeneration(Population population)
		{
			population.GenerationNumber = 0;

			for (int i = 0; i < WorldSettings.DefaultGenerationSize; i++)
			{
				var child = Genome.CreateRandom();
				PlaceChildInSpecies(population, child);
			}

			// WYLO: fix the rest of the code to use the neural network class
		}

		public static void CreateNewGeneration(Population population)
		{
			var random = new Random();

			// Sort the ants in each species by their fitness.
			foreach (var species in population.Species) { species.SortGenomesByFitness(); }

			// Remove the bottom half of each species.
			foreach (var species in population.Species) { species.RemoveGenomes(all: false); }

			// Rank all genomes globally.
			population.AssignGlobalRanks();

			// Take the highest performing ant in each species and make it the representative.
			foreach (var species in population.Species) { species.Representative = species.Genomes[0]; }

			// Recalculate the staleness of species and remove any species that are too stale.
			foreach (var species in population.Species)
			{
				var bestPerformer = species.Genomes[0];
				if (bestPerformer.Fitness > species.BestRecordedFitness)
				{
					species.BestRecordedFitness = bestPerformer.Fitness;
					species.Staleness = 0;
				}
				else { species.Staleness++; }
			}
			population.RemoveStaleSpecies();

			// Rerank all the genomes globally.
			population.AssignGlobalRanks();

			// Get the total average fitness for all species.
			var totalAverageFitness = population.Species.Sum(s => s.AverageFitness);

			var children = new List<Genome>();

			// For each species...
			foreach (var species in population.Species)
			{
				int numberToBreed = (int)((species.AverageFitness / totalAverageFitness) *
					WorldSettings.DefaultGenerationSize) - 1;

				for (int i = 0; i < numberToBreed; i++)
				{
					children.Add(CreateChild(species));
				}

				// Remove all members of all species.
				species.RemoveGenomes(all: true);
			}

			// Breed children from random species' representatives until the population fills out.
			while (children.Count < WorldSettings.DefaultGenerationSize)
			{
				var randomSpecies = GlobalRandom.ChooseRandom(population.Species);
				children.Add(CreateChild(randomSpecies));
			}

			foreach (var child in children) { PlaceChildInSpecies(population, child); }

			population.GenerationNumber++;
		}

		private static Genome CreateChild(Species species)
		{
			var child = new Genome();

			if (species.Genomes.Any())
			{
				if (GlobalRandom.NextDouble() < WorldSettings.CrossoverOdds)
				{
					var g1 = species.Genomes[GlobalRandom.Next(species.Genomes.Count)];
					Genome g2 = species.Genomes[GlobalRandom.Next(species.Genomes.Count)];
					while (ReferenceEquals(g1, g2)) { g2 = species.Genomes[GlobalRandom.Next(species.Genomes.Count)]; }

					child = Genome.Crossover(child, g1, g2);
				}
				else
				{
					var parent = species.Genomes[GlobalRandom.Next(species.Genomes.Count)];
					child = Genome.DirectClone(child, parent);
				}
			}
			else
			{
				child = Genome.DirectClone(child, species.Representative);
			}

			if (GlobalRandom.NextDouble() < WorldSettings.MutationOdds)
			{
				child.Mutate();
			}

			return child;
		}

		private static void PlaceChildInSpecies(Population population, Genome child)
		{
			foreach (var species in population.Species)
			{
				var compatDistance = child.GetCompatibilityDistance(species.Representative);
				if (compatDistance <= WorldSettings.CompatibilityThreshold)
				{
					species.AddGenome(child);
					return;
				}
			}

			// If we get here, no species is a good fit for the child, so we'll make a new species
			// just for the child.
			var newSpecies = new Species();
			newSpecies.Index = population.Species.Count;
			newSpecies.Representative = child;
			newSpecies.AddGenome(child);
			population.AddSpecies(newSpecies);
		}
	}
}
