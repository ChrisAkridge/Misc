using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class PlotIndexMapping
	{
		public int Index { get; init; }
		public string Name { get; init; }
		public PlotIndexType Type { get; init; }
		public double BucketSize { get; init; }

		public int GetBucketCount(double min, double max)
		{
			var range = max - min;
			return (int)Math.Ceiling(range / BucketSize);
		}
	}
}
