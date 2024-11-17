using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct EightBitDistribution() : IFileDistribution
	{
		public long[] counts = new long[256];
	}
}
