using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public class FourBitDistribution : IFileDistribution
	{
		private readonly long[] counts;
		
		public IReadOnlyList<long> Counts => counts;

		public FourBitDistribution(long[] counts)
		{
			if (counts.Length != 16)
			{
				throw new ArgumentException("There must be 16 counts.");
			}
			
			this.counts = counts;
		}
	}
}
