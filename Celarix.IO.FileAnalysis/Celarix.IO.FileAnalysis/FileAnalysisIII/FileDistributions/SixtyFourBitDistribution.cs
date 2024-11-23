using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class SixtyFourBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<ulong> bucketedDistribution = new(() => ulong.MaxValue,
			(v, d) => v / (ulong)d,
			(a, b) => a > b,
			v => int.CreateTruncating(v),
			(v, d) => v / d,
			(a, b) => a + b,
			() => 1);
		
		public void Add(ulong value)
		{
			bucketedDistribution.Add(value);
		}

		public int SizeOnDisk => bucketedDistribution.SizeOnDisk;

		public IFileDistribution Read(BinaryReader reader)
		{
			var distribution = new SixtyFourBitDistribution();
			distribution.bucketedDistribution.Read(reader);

			return distribution;
		}

		public void Write(BinaryWriter writer)
		{
			bucketedDistribution.Write(writer);
		}
	}
}
