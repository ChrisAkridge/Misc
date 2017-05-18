using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal static class AntCreator
	{
		public static Ant CreateRandomAnt(World world)
		{
			Ant ant = new Ant(world);
			var hiddenNeuronLayer = NeuronLayer.CreateRandomAndConnect(ant.InputNeurons, 
				ant.OutputNeurons, world.Settings);
			ant.HiddenLayer = hiddenNeuronLayer;
			return ant;
		}

		public static IReadOnlyList<Ant> CreateRandomGeneration(World world)
		{
			return CreateRandomAnts(world, world.Settings.GenerationSize);
		}

		public static IReadOnlyList<Ant> CreateRandomAnts(World world, int count)
		{
			var result = new Ant[count];

			for (int i = 0; i < count; i++)
			{
				result[i] = CreateRandomAnt(world);
			}

			return Array.AsReadOnly(result);
		}

		private static Ant Breed(Ant father, Ant mother)
		{
			Ant child = new Ant(father.World);
			child.HiddenLayer = NeuronLayer.MergeHiddenLayers(father.HiddenLayer, mother.HiddenLayer, 
				father.World.Settings);
			child.RewireConnectionsToOutputNeurons();
			child.RewireConnectionsToInputNeurons(father, mother);

			return child;
		}

		public static IEnumerable<Ant> BreedPairs(IEnumerable<Tuple<Ant, Ant>> pairs)
		{
			var result = new List<Ant>();
			foreach (var pair in pairs)
			{
				result.Add(Breed(pair.Item1, pair.Item2));
			}
			return result;
		}
	}
}
