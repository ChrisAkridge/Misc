using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal sealed class Species
	{
		private List<Genome> genomes = new List<Genome>();

		public int Index { get; internal set; }
		public Genome Representative { get; internal set; }
		public int Staleness { get; internal set; }
		public double BestRecordedFitness { get; internal set; }
		public double AverageFitness => genomes.Average(g => g.Fitness);

		public IReadOnlyList<Genome> Genomes => genomes.AsReadOnly();

		public void SortGenomesByFitness()
		{
			genomes = genomes.OrderByDescending(g => g.Fitness).ToList();
		}

		public void AddGenome(Genome genome) => genomes.Add(genome);

		public void RemoveGenomes(bool all)
		{
			if (all)
			{
				genomes.Clear();
			}
			else
			{
				int half = genomes.Count / 2;
				genomes = genomes.Take(half).ToList();
			}
		}
	}
}
