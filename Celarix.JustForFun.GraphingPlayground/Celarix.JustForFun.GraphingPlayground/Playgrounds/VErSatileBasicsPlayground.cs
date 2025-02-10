using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models;
using Celarix.JustForFun.GraphingPlayground.Models.CSVMaps;
using ScottPlot.Plottables;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground.Playgrounds
{
	internal sealed class VErSatileBasicsPlayground : IPlayground
	{
		private VErSatileBasics[] rows;
		private Dictionary<string, Action<FormsPlot>> views;
		private readonly List<PlotIndexMapping> indexMappings = [];
		private readonly Dictionary<string, GraphProperties> graphProperties = [];

		public string Name => "VErSatile Basics";
		public AdditionalSupport AdditionalSupport { get; private set; }
		public IReadOnlyList<PlotIndexMapping> IndexMappings => indexMappings;
		public IReadOnlyDictionary<string, GraphProperties> GraphProperties => graphProperties;

		public void Load(PlaygroundLoadArguments loadArguments)
		{
			var csvText = File.ReadAllText(loadArguments.FilePath);
			var reader = new CSVReader();
			
			rows = reader.GetRows<VErSatileBasics, VErSatileBasicsMap>(csvText);

			views = new Dictionary<string, Action<FormsPlot>>
			{
				["Bluebell Hours"] = p => ViewHoursByDateByType(p, "Bluebell"),
				["Crimson Hours"] = p => ViewHoursByDateByType(p, "Crimson"),
				["Emerald Hours"] = p => ViewHoursByDateByType(p, "Emerald"),
				["Sapphire Hours"] = p => ViewHoursByDateByType(p, "Sapphire"),
				["Starflower Hours"] = p => ViewHoursByDateByType(p, "Starflower"),
				["Seashell Hours"] = p => ViewHoursByDateByType(p, "Seashell"),
				["Tachycardia Hours"] = p => ViewHoursByDateByType(p, "Tachycardia"),
				["Sleep Hours"] = p => ViewHoursByDateByType(p, "Sleep"),
				["Weight"] = ViewWeightByDate,
				["Morning Blood Pressure"] = ViewMorningBloodPressureByDate
			};
		}
		
		public Action<FormsPlot> GetView(string actionName) => views[actionName];
		public string[] GetViewNames() => views.Keys.ToArray();

		private void ViewHoursByDateByType(FormsPlot formsPlot, string hoursType)
		{
			indexMappings.Clear();
			graphProperties.Clear();
			
			var dates = rows.Select(r => DateTime.Parse(r.Date)).ToList();
			var hours = rows.Select(r =>
				{
					var hoursText = hoursType switch
					{
						"Bluebell" => r.BluebellHours,
						"Crimson" => r.CrimsonHours,
						"Emerald" => r.EmeraldHours,
						"Sapphire" => r.SapphireHours,
						"Starflower" => r.StarflowerHours,
						"Seashell" => r.SeashellHours,
						"Tachycardia" => r.HoursInTachycardia,
						"Sleep" => r.SleepHours,
						_ => throw new ArgumentException($"Unknown hours type: {hoursType}")
					};

					return double.TryParse(hoursText, out var parsedHours) ? parsedHours : 0d;
				})
				.ToList();

			TrimZeroes(dates, hours);

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Scatter(dates, hours);
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title($"{hoursType} Hours");
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression
				| AdditionalSupport.RollingAverage
				| AdditionalSupport.Distribution;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = $"{hoursType} Hours",
				Type = PlotIndexType.Data,
				BucketSize = 0.25d
			});
			graphProperties["Hours"] = new GraphProperties(GraphPropertyType.Numeric, hours, 0.25d);
		}
		
		private void ViewWeightByDate(FormsPlot formsPlot)
		{
			indexMappings.Clear();
			graphProperties.Clear();
			
			var dates = rows.Select(r => DateTime.Parse(r.Date)).ToList();
			var weights = new List<double>();
			var lastWeight = 0d;
			
			foreach (var row in rows)
			{
				var parsedWeight = double.TryParse(row.Weight, out var weight)
					? weight
					: 0d;
				weights.Add(parsedWeight);
			}

			TrimZeroes(dates, weights);

			for (var i = 0; i < weights.Count; i++)
			{
				if (weights[i] != 0)
				{
					lastWeight = weights[i];
				}
				else
				{
					weights[i] = lastWeight;
				}
			}

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Scatter(dates, weights);
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title("Weight");
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression
				| AdditionalSupport.RollingAverage
				| AdditionalSupport.Distribution;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = "Weight",
				Type = PlotIndexType.Data,
				BucketSize = 5d
			});
			graphProperties["Weight (lbs.)"] = new GraphProperties(GraphPropertyType.Numeric, weights, 5d);
		}

		private void ViewMorningBloodPressureByDate(FormsPlot formsPlot)
		{
			indexMappings.Clear();
			
			var dates = rows.Select(r => DateTime.Parse(r.Date)).ToList();
			var diastolicPressures = new List<double>();
			var systolicPressures = new List<double>();
			var lastDiastolicPressure = 0d;
			var lastSystolicPressure = 0d;
			
			foreach (var row in rows)
			{
				var parsedDiastolicPressure = double.TryParse(row.MorningDiastolicBloodPressure, out var diastolicPressure)
					? diastolicPressure
					: 0d;
				diastolicPressures.Add(parsedDiastolicPressure);
				
				var parsedSystolicPressure = double.TryParse(row.MorningSystolicBloodPressure, out var systolicPressure)
					? systolicPressure
					: 0d;
				systolicPressures.Add(parsedSystolicPressure);
			}
			
			TrimZeroes(dates, diastolicPressures, systolicPressures);

			for (int i = 0; i < dates.Count; i++)
			{
				if (diastolicPressures[i] != 0)
				{
					lastDiastolicPressure = diastolicPressures[i];
				}
				else
				{
					diastolicPressures[i] = lastDiastolicPressure;
				}
				
				if (systolicPressures[i] != 0)
				{
					lastSystolicPressure = systolicPressures[i];
				}
				else
				{
					systolicPressures[i] = lastSystolicPressure;
				}
			}
			
			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Scatter(dates, diastolicPressures, ScottPlot.Color.FromColor(Color.Red));
			formsPlot.Plot.Add.Scatter(dates, systolicPressures, ScottPlot.Color.FromColor(Color.Lime));
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title("Morning Blood Pressure");
			formsPlot.Refresh();
			
			AdditionalSupport = AdditionalSupport.LinearRegression
				| AdditionalSupport.RollingAverage
				| AdditionalSupport.Distribution;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = "Morning Diastolic Blood Pressure",
				Type = PlotIndexType.Data,
				BucketSize = 5d
			});
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 1,
				Name = "Morning Systolic Blood Pressure",
				Type = PlotIndexType.Data,
				BucketSize = 5d
			});

			graphProperties["Diastolic Pressure"] =
				new GraphProperties(GraphPropertyType.Numeric, diastolicPressures, 5d);
			graphProperties["Systolic Pressure"] =
				new GraphProperties(GraphPropertyType.Numeric, systolicPressures, 5d);
		}

		private void TrimZeroes<TXAxis>(List<TXAxis> xAxis, List<double> yAxis)
		{
			while (yAxis.First() == 0 && yAxis.Count > 1)
			{
				xAxis.RemoveAt(0);
				yAxis.RemoveAt(0);
			}
			
			while (yAxis.Last() == 0 && yAxis.Count > 1)
			{
				xAxis.RemoveAt(xAxis.Count - 1);
				yAxis.RemoveAt(yAxis.Count - 1);
			}
		}
		
		private void TrimZeroes<TXAxis>(List<TXAxis> xAxis, List<double> yAxis1, List<double> yAxis2)
		{
			while (yAxis1.First() == 0 && yAxis2.First() == 0 && yAxis1.Count > 1)
			{
				xAxis.RemoveAt(0);
				yAxis1.RemoveAt(0);
				yAxis2.RemoveAt(0);
			}
			
			while (yAxis1.Last() == 0 && yAxis2.Last() == 0 && yAxis1.Count > 1)
			{
				xAxis.RemoveAt(xAxis.Count - 1);
				yAxis1.RemoveAt(yAxis1.Count - 1);
				yAxis2.RemoveAt(yAxis2.Count - 1);
			}
		}
	}
}
