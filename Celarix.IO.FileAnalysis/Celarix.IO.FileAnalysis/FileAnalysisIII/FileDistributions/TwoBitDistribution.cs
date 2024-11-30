using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public class TwoBitDistribution(long _00, long _01, long _10, long _11)
		: IFileDistribution
	{
		private readonly long[] counts =
		[
			_00, _01, _10, _11
		];
		
		public int SizeOnDisk => 4 * sizeof(long);

		public IFileDistribution Read(BinaryReader reader) =>
			new TwoBitDistribution
			(
				reader.ReadInt64(),
				reader.ReadInt64(),
				reader.ReadInt64(),
				reader.ReadInt64()
			);

		public void Write(BinaryWriter writer)
		{
			writer.Write(counts[0]);
			writer.Write(counts[1]);
			writer.Write(counts[2]);
			writer.Write(counts[3]);
		}
	}
}
