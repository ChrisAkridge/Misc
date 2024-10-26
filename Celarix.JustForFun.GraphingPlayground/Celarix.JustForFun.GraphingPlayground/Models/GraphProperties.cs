using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class GraphProperties
	{
		private static readonly decimal[] DefaultPercentiles =
		[
			0.01m, 0.10m, 0.25m, 0.50m, 0.75m,
			0.90m, 0.99m
		];
		
		private readonly Dictionary<decimal, double> percentiles;
		
		public GraphPropertyType PropertyType { get; }
		public double MinValue { get; }
		public double MaxValue { get; }
		public double Mean { get; }
		public double Median { get; }
		public double Mode { get; }
		public double StandardDeviation { get; }
		public IReadOnlyDictionary<decimal, double> Percentiles => percentiles;
		
		public double DistributionBucketSize { get; set; }
		
		public GraphProperties(GraphPropertyType propertyType, IReadOnlyList<double> values,
			double distributionBucketSize)
		{
			PropertyType = propertyType;
			DistributionBucketSize = distributionBucketSize;

			var sortedValues = values.OrderBy(v => v).ToArray();

			MinValue = sortedValues.First();
			MaxValue = sortedValues.Last();
			Mean = values.Average();
			Median = CalculateMedian(sortedValues);
			Mode = CalculateMode(values);
			StandardDeviation = CalculateStandardDeviation(values);
			percentiles = CalculatePercentiles(sortedValues);
		}
		
		private static double CalculateMedian(IReadOnlyList<double> values)
		{
			var count = values.Count;

			if (count % 2 != 0) { return values[count / 2]; }

			var midIndex = count / 2;
			return (values[midIndex - 1] + values[midIndex]) / 2;
		}
		
		private static double CalculateMode(IEnumerable<double> values)
		{
			var mode = values.GroupBy(v => v).MaxBy(g => g.Count());

			return mode?.Key ?? double.NaN;
		}
		
		private static double CalculateStandardDeviation(IReadOnlyList<double> values)
		{
			var mean = values.Average();
			var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));

			return Math.Sqrt(sumOfSquares / values.Count);
		}
		
		private static Dictionary<decimal, double> CalculatePercentiles(IReadOnlyList<double> values)
		{
			var percentiles = new Dictionary<decimal, double>();

			foreach (var percentile in DefaultPercentiles)
			{
				var index = (int)(percentile * values.Count);
				percentiles[percentile] = values[index];
			}

			return percentiles;
		}
	}
}
