using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class ThirtyTwoBitDistribution : IFileDistribution
	{
		private readonly BSPTreeNode<UInt32WithExtraInterfaces> root =
			new(UInt32WithExtraInterfaces.MinValue, UInt32WithExtraInterfaces.MaxValue);

		public void Add(uint value)
		{
			root.Add(new UInt32WithExtraInterfaces(value));
		}
	}
}
