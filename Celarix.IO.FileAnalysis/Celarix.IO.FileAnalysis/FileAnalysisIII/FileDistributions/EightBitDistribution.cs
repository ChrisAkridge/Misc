using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct EightBitDistribution() : IFileDistribution
	{
		public long[] counts = new long[256];

		public int SizeOnDisk => 256 * sizeof(long);

		public IFileDistribution Read(BinaryReader reader)
		{
			var distribution = new EightBitDistribution();
			for (int i = 0; i < 256; i++)
			{
				distribution.counts[i] = reader.ReadInt64();
			}
			return distribution;
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
