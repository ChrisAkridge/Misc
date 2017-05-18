using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class WorldSettings
	{
		public int GenerationSize { get; set; }
		public int InitialAntLifespan { get; set; }
		public int AdditionalLifespanFromFood { get; set; }
		public int TopNAntsSelectedForBreeding { get; set; }
		public double MutationOdds { get; set; }

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
			MutationOdds = 0.01d;

			WallSpawnOdds = 0.1d;
			FoodSpawnOdds = 0.25d;

			MaxRandomNeuronsPerNewAnt = 20;
			MaxRandomNeuronFireStrength = 10d;
			MaxRandomNeuronFireRequirementBaseline = 10d;
			MaxRandomNeuronFireFatigueIncrease = 0.8d;
			MaxRandomNeuronFireFatigueDecrease = 0.9d;
			MaxRandomConnectionsBetweenNeurons = 5;
		}

		// WYLO: next up: breeding
		// 1. select the top n ants by lifespan from a generation (where N is even)
		// 2. pair each ant in the top group randomly
		// 3. given two ants,
		//	*. create a new ant with no hidden neurons
		//	a. select half the hidden neurons at random
		//	b. extract them into a separate collection
		//	c. sever outbound connections to neurons not in the new collection
		//	d. keep connections to neurons that are in the new collection or to output neurons
		//	e. combine the neurons from the two collections (one collection) from each ant
		//	f. spawn new connections between the two groups
		//	g. rewire connections to new output neurons and from the new input neurons
		// 4. any ants not made from breeding will be randomly generated
	}
}
