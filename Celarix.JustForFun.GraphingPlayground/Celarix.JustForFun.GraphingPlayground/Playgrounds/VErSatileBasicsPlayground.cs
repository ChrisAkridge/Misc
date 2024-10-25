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

		public string Name => "VErSatile Basics";
		public AdditionalSupport AdditionalSupport { get; private set; }
		public IReadOnlyList<PlotIndexMapping> IndexMappings => IndexMappings;

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
				["Sleep Hours"] = p => ViewHoursByDateByType(p, "Sleep")
			};
		}
		
		public Action<FormsPlot> GetView(string actionName) => views[actionName];
		public string[] GetViewNames() => views.Keys.ToArray();

		private void ViewHoursByDateByType(FormsPlot formsPlot, string hoursType)
		{
			indexMappings.Clear();
			
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

			while (hours.First() == 0 && hours.Count > 1)
			{
				dates.RemoveAt(0);
				hours.RemoveAt(0);
			}
			
			while (hours.Last() == 0 && hours.Count > 1)
			{
				dates.RemoveAt(dates.Count - 1);
				hours.RemoveAt(hours.Count - 1);
			}

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Scatter(dates, hours);
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title($"{hoursType} Hours");
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression
				| AdditionalSupport.RollingAverage;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = $"{hoursType} Hours",
				Type = PlotIndexType.Data
			});
		}
	}
}
