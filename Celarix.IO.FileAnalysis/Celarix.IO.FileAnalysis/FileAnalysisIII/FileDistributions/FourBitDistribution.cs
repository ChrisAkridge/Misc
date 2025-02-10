using System;
using System.Collections.Generic;
using System.IO;
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
		
		public int SizeOnDisk => 16 * sizeof(long);
		
		public IFileDistribution Read(BinaryReader reader)
		{
			var distribution = new long[16];
			for (int i = 0; i < 16; i++)
			{
				distribution[i] = reader.ReadInt64();
			}
			return new FourBitDistribution(distribution);
		}

		public void Write(BinaryWriter writer)
		{
			foreach (long count in counts)
			{
				writer.Write(count);
			}
		}
	}
}
