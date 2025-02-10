using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct OneBitDistribution : IFileDistribution
	{
		public long Bit0Count;
		public long Bit1Count;
		
		public int SizeOnDisk => 2 * sizeof(long);
		
		public IFileDistribution Read(BinaryReader reader) =>
			new OneBitDistribution
			{
				Bit0Count = reader.ReadInt64(),
				Bit1Count = reader.ReadInt64()
			};

		public void Write(BinaryWriter writer)
		{
			writer.Write(Bit0Count);
			writer.Write(Bit1Count);
		}
	}
}
