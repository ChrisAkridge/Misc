using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct OneBitDistribution : IFileDistribution
	{
		public long Bit0Count;
		public long Bit1Count;
	}
}
