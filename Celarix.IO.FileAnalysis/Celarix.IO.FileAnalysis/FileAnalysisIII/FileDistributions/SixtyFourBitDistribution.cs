using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class SixtyFourBitDistribution : IFileDistribution
	{
		private readonly BSPTreeNode<UInt64WithExtraInterfaces> root =
			new(UInt64WithExtraInterfaces.MinValue, UInt64WithExtraInterfaces.MaxValue);

		public void Add(ulong value)
		{
			root.Add(new UInt64WithExtraInterfaces(value));
		}
	}
}
