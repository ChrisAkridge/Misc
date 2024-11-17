using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class TwentyFourBitDistribution : IFileDistribution
	{
		private readonly BSPTreeNode<int> root = new BSPTreeNode<int>(0, 16_777_215);

		public void AddSixteen(int _0,
			int _1,
			int _2,
			int _3,
			int _4,
			int _5,
			int _6,
			int _7,
			int _8,
			int _9,
			int _A,
			int _B,
			int _C,
			int _D,
			int _E,
			int _F)
		{
			root.Add(_0);
			root.Add(_1);
			root.Add(_2);
			root.Add(_3);
			root.Add(_4);
			root.Add(_5);
			root.Add(_6);
			root.Add(_7);
			root.Add(_8);
			root.Add(_9);
			root.Add(_A);
			root.Add(_B);
			root.Add(_C);
			root.Add(_D);
			root.Add(_E);
			root.Add(_F);
		}
	}
}
