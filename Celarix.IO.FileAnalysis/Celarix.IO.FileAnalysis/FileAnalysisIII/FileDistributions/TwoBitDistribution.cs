using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public class TwoBitDistribution(long _00, long _01, long _10, long _11)
		: IFileDistribution
	{
		private long[] counts =
		[
			_00, _01, _10, _11
		];

		public IReadOnlyList<long> Counts => counts;
	}
}
