using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	[StructLayout(LayoutKind.Sequential)]
	public struct TwoBitDistribution : IFileDistribution
	{
		public long BitPair00Count;
		public long BitPair01Count;
		public long BitPair10Count;
		public long BitPair11Count;
	}
}
