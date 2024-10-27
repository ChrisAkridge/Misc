using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Logic;
using Celarix.JustForFun.GraphingPlayground.Models;
using ScottPlot;
using ScottPlot.Colormaps;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground.Playgrounds
{
	internal sealed class SingleFileDistribution : IPlayground
	{
		private Dictionary<string, Action<FormsPlot>> views;
		private readonly List<PlotIndexMapping> indexMappings = [];
		private readonly Dictionary<string, GraphProperties> graphProperties = new();

		private FileDistribution fileDistribution;
		
		public string Name => "Single File Distribution";
		public AdditionalSupport AdditionalSupport { get; private set; }
		public IReadOnlyList<PlotIndexMapping> IndexMappings => indexMappings;
		public IReadOnlyDictionary<string, GraphProperties> GraphProperties => graphProperties;

		public void Load(PlaygroundLoadArguments loadArguments)
		{
			fileDistribution = new FileDistribution(loadArguments.FilePath);

			views = new Dictionary<string, Action<FormsPlot>>
			{
				["Bits"] = p => BarPlotDistribution(p, fileDistribution.OneBitDistribution, "Bits"),
				["Bit Pairs"] = p => BarPlotDistribution(p, fileDistribution.TwoBitDistribution, "Bit Pairs"),
				["Nybbles"] = p => BarPlotDistribution(p, fileDistribution.FourBitDistribution, "Nybbles"),
				["Bytes"] = p => BarPlotDistribution(p, fileDistribution.EightBitDistribution, "Bytes"),
				["Byte Heatmap"] = p => HeatmapDistribution(p, fileDistribution.EightBitDistribution, "Byte Heatmap"),
				["16-bit Values"] = p => BucketedBarPlotDistribution(p, fileDistribution.SixteenBitDistribution, 64, "16-bit Values"),
				["16-bit Value Heatmap"] = p => BucketedHeatMapDistribution(p, fileDistribution.BucketByTotalDistribution(fileDistribution.SixteenBitDistribution, 256), 16, "16-bit Value Heatmap")
			};
		}

		public Action<FormsPlot> GetView(string actionName) => views[actionName];

		public string[] GetViewNames() => views.Keys.ToArray();
		
		public void BarPlotDistribution(FormsPlot formsPlot, IReadOnlyList<long> distribution, string title)
		{
			indexMappings.Clear();
			graphProperties.Clear();
			
			var values = new double[distribution.Count];
			for (int i = 0; i < distribution.Count; i++)
			{
				values[i] = distribution[i];
			}

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Bars(values);
			formsPlot.Plot.Title(title);

			var tickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.Sub256TickFormatter
			};
			formsPlot.Plot.Axes.Bottom.TickGenerator = tickGenerator;
			formsPlot.Refresh();

			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = title,
				Type = PlotIndexType.Data,
				BucketSize = 1d
			});
			graphProperties[title] = new GraphProperties(GraphPropertyType.Numeric, values, 1d);
		}

		public void BucketedBarPlotDistribution(FormsPlot formsPlot, IReadOnlyList<long> distribution, int bucketSize, string title)
		{
			title = $"{title} (Bucketed by {bucketSize})";
			
			indexMappings.Clear();
			graphProperties.Clear();
			
			var buckets = new double[distribution.Count / bucketSize];
			for (int i = 0; i < distribution.Count; i++)
			{
				var bucketIndexForValue = i / bucketSize;
				buckets[bucketIndexForValue] += distribution[i];
			}
			
			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Bars(buckets);
			formsPlot.Plot.Title(title);

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Bars(buckets);
			formsPlot.Plot.Title(title);
			
			var tickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.BucketedTickFormatterGenerator(bucketSize)
			};
			formsPlot.Plot.Axes.Bottom.TickGenerator = tickGenerator;
			formsPlot.Refresh();

			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0, Name = title, Type = PlotIndexType.Data, BucketSize = 1d
			});
			graphProperties[title] = new GraphProperties(GraphPropertyType.Numeric, buckets, 1d);
		}
		
		public void HeatmapDistribution(FormsPlot formsPlot, IReadOnlyList<long> distribution, string title)
		{
			indexMappings.Clear();
			graphProperties.Clear();
			
			// Take the square root of the distribution length to figure out the size of the X and Y axes
			var edgeLength = (int)Math.Sqrt(distribution.Count);
			var values = new double[edgeLength, edgeLength];
			
			for (int i = 0; i < distribution.Count; i++)
			{
				var x = i % edgeLength;
				var y = i / edgeLength;
				values[x, y] = distribution[i];
			}
			
			formsPlot.Plot.Clear();
			var heatmap = formsPlot.Plot.Add.Heatmap(values);
			heatmap.Extent = new CoordinateRect(0d, edgeLength, 0d, edgeLength);
			formsPlot.Plot.Title(title);
			
			var yTickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.HeatmapHighPartTickFormatterGenerator(distribution.Count)
			};
			
			var xTickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.HeatmapLowPartTickFormatterGenerator(distribution.Count)
			};
			
			formsPlot.Plot.Axes.Bottom.TickGenerator = xTickGenerator;
			formsPlot.Plot.Axes.Left.TickGenerator = yTickGenerator;
			formsPlot.Refresh();
			
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0, Name = title, Type = PlotIndexType.Data, BucketSize = 1d
			});
			graphProperties[title] = new GraphProperties(GraphPropertyType.Numeric, values.Flatten2DArray(), 1d);
		}

		public void BucketedHeatMapDistribution(FormsPlot formsPlot, double[,] buckets, int spaceSizeInBits, string title)
		{
			indexMappings.Clear();
			graphProperties.Clear();

			var halfBucketSizeInBits = (int)Math.Log2(buckets.GetLength(0));
			var bucketSizeInBits = halfBucketSizeInBits * 2;

			title = $"{title} (Bucketed by {Math.Pow(2d, bucketSizeInBits):#,###})";

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Heatmap(buckets);
			formsPlot.Plot.Title(title);
			
			var yTickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.BucketedHeatmapHighPartTickFormatterGenerator(spaceSizeInBits, bucketSizeInBits)
			};
			
			var xTickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = HexTickGenerators.BucketedHeatmapLowPartTickFormatterGenerator(spaceSizeInBits, bucketSizeInBits)
			};
			
			formsPlot.Plot.Axes.Bottom.TickGenerator = xTickGenerator;
			formsPlot.Plot.Axes.Left.TickGenerator = yTickGenerator;
			formsPlot.Refresh();
			
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0, Name = title, Type = PlotIndexType.Data, BucketSize = 1d
			});
			graphProperties[title] = new GraphProperties(GraphPropertyType.Numeric, buckets.Flatten2DArray(), 1d);
		}
	}
}
