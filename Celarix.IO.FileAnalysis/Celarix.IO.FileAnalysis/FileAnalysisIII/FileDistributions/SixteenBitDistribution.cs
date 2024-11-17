﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public struct SixteenBitDistribution() : IFileDistribution
	{
		public long[] counts = new long[65536];
	}
}
