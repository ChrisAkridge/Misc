using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Celarix.JustForFun.GraphingPlayground.Models;
using ScottPlot;
using ScottPlot.AxisRules;
using ScottPlot.Plottables;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal partial class PlaygroundForm : Form
	{
		private readonly IPlayground playground;
		private readonly List<PlotIndexMapping> indexMappings = new();

		public PlaygroundForm(IPlayground playground)
		{
			this.playground = playground;
			InitializeComponent();
		}

		private void TSBOpenFile_Click(object sender, EventArgs e)
		{
			if (OFDMain.ShowDialog() != DialogResult.OK) { return; }

			var loadArguments = new PlaygroundLoadArguments
			{
				FilePath = OFDMain.FileName
			};

			playground.Load(loadArguments);

			var viewNames = playground.GetViewNames();

			foreach (var viewName in viewNames)
			{
				TSDDBViews.DropDownItems.Add(viewName, null, (_, _) => SwitchToView(viewName));
			}
		}

		private void Reset(bool resetGraphProperties = true)
		{
			PlotMain.Reset();
			indexMappings.Clear();
			ComboGraphPropertiesList.Items.Clear();
			SetGraphProperties(null);
			ComboDistributionGraph.Items.Clear();
		}

		private void SwitchToView(string viewName)
		{
			Reset();

			playground.GetView(viewName)(PlotMain);

			indexMappings.AddRange(playground.IndexMappings);

			foreach (var graphPropertiesName in playground.GraphProperties.Keys)
			{
				ComboGraphPropertiesList.Items.Add(graphPropertiesName);
			}

			foreach (var indexMapping in indexMappings)
			{
				ComboDistributionGraph.Items.Add(indexMapping.Name);
			}

			SetAdditionalSupportControlsEnabled();
		}

		private void SetAdditionalSupportControlsEnabled()
		{
			var additionalSupport = playground.AdditionalSupport;

			CheckLinearRegression.Enabled = additionalSupport.HasFlag(AdditionalSupport.LinearRegression);
			CheckRollingAverage.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
			StaticLabelRollingAveragePeriod.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
			NUDRollingAveragePeriod.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
			ButtonToDistribution.Enabled = additionalSupport.HasFlag(AdditionalSupport.Distribution);
			ComboDistributionGraph.Enabled = additionalSupport.HasFlag(AdditionalSupport.Distribution);
		}

		private void SetAxisRules()
		{
			PlotMain.Plot.Axes.Rules.Clear();
			var limits = PlotMain.Plot.Axes.GetLimits();

			if (CheckLockXAxis.CheckState == CheckState.Checked)
			{
				var rule = new LockedHorizontal(PlotMain.Plot.Axes.Bottom, limits.Left, limits.Right);
				PlotMain.Plot.Axes.Rules.Add(rule);
			}

			if (CheckLockYAxis.CheckState == CheckState.Checked)
			{
				var rule = new LockedVertical(PlotMain.Plot.Axes.Left, limits.Bottom, limits.Top);
				PlotMain.Plot.Axes.Rules.Add(rule);
			}

			PlotMain.Refresh();
		}

		private void CheckLockXAxis_CheckedChanged(object sender, EventArgs e) => SetAxisRules();

		private void CheckLockYAxis_CheckedChanged(object sender, EventArgs e) => SetAxisRules();

		private void CheckLinearRegression_CheckedChanged(object sender, EventArgs e)
		{
			if (CheckLinearRegression.CheckState == CheckState.Checked)
			{
				BuildLinearRegressionsForScatterPlots();
			}
			else
			{
				PlotMain.Plot.Remove<LinePlot>();
				indexMappings.RemoveAll(m => m.Type == PlotIndexType.LinearRegression);
				PlotMain.Plot.Title(playground.Name);
			}

			PlotMain.Refresh();
		}

		private void CheckRollingAverage_CheckedChanged(object sender, EventArgs e) { RollingAverageControlUpdated(); }

		private void NUDRollingAveragePeriod_ValueChanged(object sender, EventArgs e)
		{
			RollingAverageControlUpdated();
		}

		private void RollingAverageControlUpdated()
		{
			if (CheckRollingAverage.CheckState == CheckState.Checked) { BuildRollingAverages(); }
			else
			{
				var rollingAveragesToRemove = indexMappings
					.Where(m => m.Type == PlotIndexType.RollingAverage)
					.Select(m => m.Index)
					.ToArray();
				PlotMain.Plot.PlottableList.RemoveAtIndices(rollingAveragesToRemove);
				indexMappings.RemoveAll(m => m.Type == PlotIndexType.RollingAverage);
			}

			PlotMain.Refresh();
		}

		private void ComboGraphPropertiesList_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (ComboGraphPropertiesList.SelectedItem is string item)
			{
				var properties = playground.GraphProperties[item];
				SetGraphProperties(properties);
			}
		}

		private void SetGraphProperties(GraphProperties? properties)
		{
			if (properties == null)
			{
				LabelGraphProperties.Text = "Max: 0 Min: 0\r\nMean: 0 Median: 0\r\nMode: 0 StdDev: 0";

				return;
			}

			var builder = new StringBuilder();
			builder.AppendLine($"Max: {properties.MaxValue:F2} Min: {properties.MinValue:F2}");
			builder.AppendLine($"Mean: {properties.Mean:F2} Median: {properties.Median:F2}");
			builder.AppendLine($"Mode: {properties.Mode:F2} StdDev: {properties.StandardDeviation:F2}");

			foreach (var kvpPair in properties.Percentiles.Pair())
			{
				if (kvpPair.First.Key != 0m)
				{
					builder.Append($"{PercentileLabel(kvpPair.First.Key)} {kvpPair.First.Value:F2} ");
				}

				if (kvpPair.Second.Key != 0m)
				{
					builder.AppendLine($"{PercentileLabel(kvpPair.Second.Key)} {kvpPair.Second.Value:F2}");
				}
			}

			LabelGraphProperties.Text = builder.ToString();

			return;

			string PercentileLabel(decimal percentile)
			{
				var times100 = (int)(percentile * 100m);

				return $"{times100.Ordinal()} %ile:";
			}
		}

		private void ButtonToDistribution_Click(object sender, EventArgs e)
		{
			if (ComboDistributionGraph.SelectedItem is not string selectedGraphName) { return; }
			
			var selectedGraph = indexMappings.First(m => m.Name == selectedGraphName);
			BuildDistribution(selectedGraph);
		}

		#region Additional Support

		private void BuildLinearRegressionsForScatterPlots()
		{
			var titleBuilder = new StringBuilder();
			indexMappings.RemoveAll(m => m.Type == PlotIndexType.LinearRegression);

			var initialScatterPlots = PlotMain.Plot.GetPlottables<Scatter>().ToArray();

			foreach (var scatterPlot in initialScatterPlots)
			{
				var scatterPoints = scatterPlot.Data.GetScatterPoints();
				var linearRegression = new ScottPlot.Statistics.LinearRegression(scatterPoints);

				var regressionStart = new Coordinates(scatterPoints.First().X,
					linearRegression.GetValue(scatterPoints.First().X));

				var regressionEnd = new Coordinates(scatterPoints.Last().X,
					linearRegression.GetValue(scatterPoints.Last().X));
				var line = PlotMain.Plot.Add.Line(regressionStart, regressionEnd);
				line.MarkerSize = 0f;
				line.LineWidth = 2f;
				line.LinePattern = LinePattern.Dashed;

				titleBuilder.Append($"{linearRegression.FormulaWithRSquared}, ");
				indexMappings.Add(new PlotIndexMapping
				{
					Index = PlotMain.Plot.PlottableList.Count - 1,
					Name = "Linear Regression",
					Type = PlotIndexType.LinearRegression
				});
			}

			titleBuilder.Remove(titleBuilder.Length - 2, 2);
			PlotMain.Plot.Title(titleBuilder.ToString());
		}

		private void BuildRollingAverages()
		{
			var rollingAveragesToRemove = indexMappings
				.Where(m => m.Type == PlotIndexType.RollingAverage)
				.Select(m => m.Index)
				.ToArray();
			PlotMain.Plot.PlottableList.RemoveAtIndices(rollingAveragesToRemove);
			indexMappings.RemoveAll(m => m.Type == PlotIndexType.RollingAverage);

			var initialScatterPlots = PlotMain.Plot.GetPlottables<Scatter>().ToArray();

			foreach (var scatterPlot in initialScatterPlots)
			{
				var scatterPoints = scatterPlot.Data.GetScatterPoints();
				var rollingWindow = new double[(int)NUDRollingAveragePeriod.Value];
				var rollingAverages = new Coordinates[scatterPoints.Count];

				for (int i = 0; i < scatterPoints.Count; i++)
				{
					var windowCopyIndex = i;
					for (int w = rollingWindow.Length - 1; w >= 0; w--)
					{
						var coordinate = windowCopyIndex >= 0
							? scatterPoints[windowCopyIndex]
							: scatterPoints[0];
						rollingWindow[w] = coordinate.Y;
						windowCopyIndex--;
					}
					rollingAverages[i] = new Coordinates(scatterPoints[i].X, rollingWindow.Average());
				}

				PlotMain.Plot.Add.Scatter(rollingAverages);
				indexMappings.Add(new PlotIndexMapping
				{
					Index = PlotMain.Plot.PlottableList.Count - 1,
					Name = "Rolling Average",
					Type = PlotIndexType.RollingAverage
				});
			}
		}

		private void BuildDistribution(PlotIndexMapping indexMapping)
		{
			if (PlotMain.Plot.PlottableList[indexMapping.Index] is not Scatter scatterPlot)
			{
				MessageBox.Show("The selected plot is not a scatter plot.", "Invalid Plot Type", MessageBoxButtons.OK,
					MessageBoxIcon.Error);
				return;
			}

			var scatterPoints = scatterPlot.Data.GetScatterPoints();
			var values = scatterPoints.Select(p => p.Y).ToArray();
			var distribution = new ScottPlot.Statistics.Histogram(values, indexMapping.GetBucketCount(values.Min(), values.Max()));

			Reset(resetGraphProperties: false);
			var barPlot = PlotMain.Plot.Add.Bars(distribution.Bins, distribution.Counts);

			foreach (var bar in barPlot.Bars)
			{
				bar.Size = distribution.BinSize * 0.8d;
			}
			
			PlotMain.Plot.Title($"Distribution of {indexMapping.Name}");
			PlotMain.Refresh();
			ButtonToDistribution.Enabled = false;
			ComboDistributionGraph.Enabled = false;
		}
		#endregion
	}
}
