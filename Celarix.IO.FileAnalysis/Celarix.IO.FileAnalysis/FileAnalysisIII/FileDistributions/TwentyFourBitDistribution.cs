using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class TwentyFourBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<int> bucketedDistribution = new(16_777_215,
			() => 16_777_215,
			(v, d) => v / d,
			(v, m) => v > m,
			v => v,
			(v, d) => v / d,
			(a, b) => a + b,
			() => 1);

		public void AddEight(int _0, int _1, int _2, int _3, int _4,
			int _5, int _6, int _7)
		{
			bucketedDistribution.Add(_0);
			bucketedDistribution.Add(_1);
			bucketedDistribution.Add(_2);
			bucketedDistribution.Add(_3);
			bucketedDistribution.Add(_4);
			bucketedDistribution.Add(_5);
			bucketedDistribution.Add(_6);
			bucketedDistribution.Add(_7);
		}

		public int SizeOnDisk => bucketedDistribution.SizeOnDisk;

		public IFileDistribution Read(BinaryReader reader)
		{
			var distribution = new TwentyFourBitDistribution();
			distribution.bucketedDistribution.Read(reader);

			return distribution;
		}

		public void Write(BinaryWriter writer)
		{
			bucketedDistribution.Write(writer);
		}
	}
}
