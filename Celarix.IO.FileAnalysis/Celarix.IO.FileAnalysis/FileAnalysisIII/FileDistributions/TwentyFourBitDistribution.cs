using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class TwentyFourBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<int> root = new(16_777_215,
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
			root.Add(_0);
			root.Add(_1);
			root.Add(_2);
			root.Add(_3);
			root.Add(_4);
			root.Add(_5);
			root.Add(_6);
			root.Add(_7);
		}
	}
}
