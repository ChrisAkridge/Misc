﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII.FileDistributions
{
	public sealed class ThirtyTwoBitDistribution : IFileDistribution
	{
		private readonly BucketedDistribution<uint> root = new(() => uint.MaxValue,
			(v, d) => v / (uint)d,
			(a, b) => a > b,
			v => int.CreateTruncating(v),
			(v, d) => v / d,
			(a, b) => a + b,
			() => 1);

		public void Add(uint value)
		{
			root.Add(value);
		}
	}
}
