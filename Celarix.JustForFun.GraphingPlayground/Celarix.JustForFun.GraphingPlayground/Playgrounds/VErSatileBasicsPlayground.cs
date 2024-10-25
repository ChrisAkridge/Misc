using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models;
using Celarix.JustForFun.GraphingPlayground.Models.CSVMaps;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground.Playgrounds
{
	internal sealed class VErSatileBasicsPlayground : IPlayground
	{
		private VErSatileBasics[] rows;
		private Dictionary<string, Action<FormsPlot>> views;

		public string Name => "VErSatile Basics";

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
			var dates = rows.Select(r => DateTime.Parse(r.Date)).ToArray();
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

					return double.TryParse(hoursText, out var hours) ? hours : 0d;
				})
				.ToArray();

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Scatter(dates, hours);
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title($"{hoursType} Hours");
			formsPlot.Refresh();
		}
	}
}
