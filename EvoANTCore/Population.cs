using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal sealed class Population
	{
		private List<Species> species = new List<Species>();

		public IReadOnlyList<Species> Species => species.AsReadOnly();
		private int HighestSpeciesIndex { get; set; }
		public double TotalAverageFitness => species.Sum(s => s.AverageFitness);
		public int GenerationNumber { get; set; }

		public void AddSpecies(Species species) => this.species.Add(species);

		public void AssignGlobalRanks()
		{
			var genomes = species.SelectMany(s => s.Genomes)
				.OrderByDescending(g => g.Fitness);

			int rank = 0;
			foreach (var genome in genomes)
			{
				genome.GlobalRank = rank;
				rank++;
			}
		}

		public double ModifiedFitness(Genome genome)
		{
			double sum = 0d;
			foreach (var member in Species.SelectMany(s => s.Genomes))
			{
				double compatDistance = genome.GetCompatibilityDistance(member);
				if (compatDistance > WorldSettings.CompatibilityThreshold) { compatDistance = 0; }
				sum += compatDistance;
			}

			return genome.Fitness / sum;
		}

		public void RemoveStaleSpecies()
		{
			species.RemoveAll(s => s.Staleness > WorldSettings.MaximumSpeciesStaleness);
		}
	}
}
