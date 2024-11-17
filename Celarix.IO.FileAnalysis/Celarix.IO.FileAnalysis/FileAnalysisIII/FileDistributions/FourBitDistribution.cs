using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FourBitDistribution : IFileDistribution
	{
		public long Nybble0000Count;
		public long Nybble0001Count;
		public long Nybble0010Count;
		public long Nybble0011Count;
		public long Nybble0100Count;
		public long Nybble0101Count;
		public long Nybble0110Count;
		public long Nybble0111Count;
		public long Nybble1000Count;
		public long Nybble1001Count;
		public long Nybble1010Count;
		public long Nybble1011Count;
		public long Nybble1100Count;
		public long Nybble1101Count;
		public long Nybble1110Count;
		public long Nybble1111Count;
	}
}
