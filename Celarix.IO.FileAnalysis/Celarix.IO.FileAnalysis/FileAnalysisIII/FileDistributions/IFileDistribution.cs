using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public interface IFileDistribution
	{
		int SizeOnDisk { get; }
		IFileDistribution Read(BinaryReader reader);
		void Write(BinaryWriter writer);
	}
}
