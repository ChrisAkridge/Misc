using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class OneHundredTwentyEightBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<UInt128> root = new(() => UInt128.MaxValue,
			(v, d) => v / (UInt128)d,
			(a, b) => a > b,
			v => int.CreateTruncating(v),
			(v, d) => v / d,
			(a, b) => a + b,
			() => 1);

		public int SizeOnDisk => root.SizeOnDisk;
		
		public IFileDistribution Read(BinaryReader reader)
		{
			var distribution = new OneHundredTwentyEightBitDistribution();
			distribution.root.Read(reader);
			return distribution;
		}

		public void Add(ulong high, ulong low)
		{
			root.Add(((UInt128)high << 64) | low);
		}

		public void Write(BinaryWriter writer)
		{
			root.Write(writer);
		}
	}
}
