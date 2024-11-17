using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class SixtyFourBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<ulong> root = new(() => ulong.MaxValue,
			(v, d) => v / (ulong)d,
			(a, b) => a > b,
			v => int.CreateTruncating(v),
			(v, d) => v / d,
			(a, b) => a + b,
			() => 1);

		public void Add(ulong value)
		{
			root.Add(value);
		}
	}
}
