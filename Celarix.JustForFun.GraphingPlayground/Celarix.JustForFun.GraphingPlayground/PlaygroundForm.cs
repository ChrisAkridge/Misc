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

		private void SwitchToView(string viewName)
		{
			PlotMain.Reset();
			indexMappings.Clear();
			playground.GetView(viewName)(PlotMain);
			indexMappings.AddRange(playground.IndexMappings);

			SetAdditionalSupportControlsEnabled();
		}

		private void SetAdditionalSupportControlsEnabled()
		{
			var additionalSupport = playground.AdditionalSupport;

			CheckLinearRegression.Enabled = additionalSupport.HasFlag(AdditionalSupport.LinearRegression);
			CheckRollingAverage.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
			StaticLabelRollingAveragePeriod.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
			NUDRollingAveragePeriod.Enabled = additionalSupport.HasFlag(AdditionalSupport.RollingAverage);
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
		#endregion
	}
}
