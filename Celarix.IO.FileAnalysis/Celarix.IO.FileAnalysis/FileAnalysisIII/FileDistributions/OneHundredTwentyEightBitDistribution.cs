using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class OneHundredTwentyEightBitDistribution : IFileDistribution
	{
		private readonly BSPTreeNode<UInt128WithExtraInterfaces> root =
			new(UInt128WithExtraInterfaces.MinValue, UInt128WithExtraInterfaces.MaxValue);

		public void Add(UInt128 value)
		{
			root.Add(new UInt128WithExtraInterfaces(value));
		}
	}
}
