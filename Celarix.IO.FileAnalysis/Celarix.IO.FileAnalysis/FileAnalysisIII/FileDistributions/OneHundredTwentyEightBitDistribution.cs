using System;
using System.Collections.Generic;
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

		public void Add(UInt128 value)
		{
			root.Add(value);
		}
	}
}
