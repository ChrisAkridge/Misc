using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class WorldSettings
	{
		public const double CompatibilityThreshold = 3d;
		public const int MaximumSpeciesStaleness = 10;
		public const int DefaultGenerationSize = 50;
		public const double CrossoverOdds = 0.25d;
		public const double MutationOdds = 0.25d;

		public int GenerationSize { get; set; }
		public int InitialAntLifespan { get; set; }
		public int AdditionalLifespanFromFood { get; set; }
		public int TopNAntsSelectedForBreeding { get; set; }
		public double _MutationOdds { get; set; }

		public double WallSpawnOdds { get; set; }
		public double FoodSpawnOdds { get; set; }

		public int MaxRandomNeuronsPerNewAnt { get; set; }
		public double MaxRandomNeuronFireStrength { get; set; }
		public double MaxRandomNeuronFireRequirementBaseline { get; set; }
		public double MaxRandomNeuronFireFatigueIncrease { get; set; }
		public double MaxRandomNeuronFireFatigueDecrease { get; set; }
		public int MaxRandomConnectionsBetweenNeurons { get; set; }

		public WorldSettings()
		{
			GenerationSize = 2;
			InitialAntLifespan = 10;
			AdditionalLifespanFromFood = 10;
			TopNAntsSelectedForBreeding = 6;
			_MutationOdds = 0.01d;

			WallSpawnOdds = 0.1d;
			FoodSpawnOdds = 0.25d;

			MaxRandomNeuronsPerNewAnt = 20;
			MaxRandomNeuronFireStrength = 10d;
			MaxRandomNeuronFireRequirementBaseline = 10d;
			MaxRandomNeuronFireFatigueIncrease = 0.8d;
			MaxRandomNeuronFireFatigueDecrease = 0.9d;
			MaxRandomConnectionsBetweenNeurons = 5;
		}

		// WYLO: next up: better breeding
		// idunnolo? maybe do some research
		// http://stackoverflow.com/questions/31708478/how-to-evolve-weights-of-a-neural-network-in-neuroevolution
		// https://medium.com/@harvitronix/lets-evolve-a-neural-network-with-a-genetic-algorithm-code-included-8809bece164
	}
}
