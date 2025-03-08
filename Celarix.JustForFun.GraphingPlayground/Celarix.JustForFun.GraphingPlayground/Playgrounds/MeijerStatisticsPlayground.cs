using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Logic;
using Celarix.JustForFun.GraphingPlayground.Models;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground.Playgrounds
{
	internal sealed class MeijerStatisticsPlayground : IPlayground
	{
		private MeijerStatisticsProvider provider = new();
		private IReadOnlyList<MeijerDay> days;
		private IReadOnlyList<MeijerWeek> weeks;
		private IReadOnlyList<MeijerSeries> series;
		
		private Dictionary<string, Action<FormsPlot>> views;
		private readonly List<PlotIndexMapping> indexMappings = [];
		private readonly Dictionary<string, GraphProperties> graphProperties = [];
		
		public string Name => "Meijer Statistics";
		public bool NeedsFile => false;
		public AdditionalSupport AdditionalSupport { get; private set; }
		public IReadOnlyList<PlotIndexMapping> IndexMappings => indexMappings;
		public IReadOnlyDictionary<string, GraphProperties> GraphProperties => graphProperties;

		public void Load(PlaygroundLoadArguments loadArguments)
		{
			days = provider.GetDays();
			weeks = provider.GetWeeks(days);
			series = provider.GetSeries(days);

			views = new Dictionary<string, Action<FormsPlot>>
			{
				["Week Types"] = WeekTypes,
				["Worked Days by Week"] = WeekTypesByWeekNumber,
				["Working Days Over Time"] = p => DaysByType(p, MeijerDayType.Working),
				["Non-Working Days Over Time"] = p => DaysByType(p, MeijerDayType.NonWorking),
				["Paid Time Off Days Over Time"] = p => DaysByType(p, MeijerDayType.PaidTimeOff),
				["Unpaid Time Off Days Over Time"] = p => DaysByType(p, MeijerDayType.NonPaidTimeOff),
				["Other Days By Time"] = p => DaysByType(p, MeijerDayType.Other),
				["Start Time by Day"] = StartTimeByDay
			};
		}

		public Action<FormsPlot> GetView(string actionName) => views[actionName];
		public string[] GetViewNames() => views.Keys.ToArray();

		private void WeekTypes(FormsPlot formsPlot)
		{
			indexMappings.Clear();
			graphProperties.Clear();

			var weekTypes = weeks.GroupBy(w => w.WorkedDays)
				.Select(g => new
				{
					g.Key,
					Name = $"{g.Key}-{7 - g.Key}",
					Count = g.Count()
				})
				.OrderBy(t => t.Key)
				.ToArray();
			
			formsPlot.Plot.Clear();
			var pieChart = formsPlot.Plot.Add.Pie(weekTypes.Select(t => (double)t.Count));

			for (var i = 0; i < pieChart.Slices.Count; i++)
			{
				// Hope that the indices match up
				var weekType = weekTypes[i];
				var pieSlice = pieChart.Slices[i];
				pieSlice.Label = $"{weekType.Name} ({weekType.Count})";
				pieSlice.LabelFontSize = 14f;
			}

			formsPlot.Refresh();
		}

		private void WeekTypesByWeekNumber(FormsPlot formsPlot)
		{
			indexMappings.Clear();
			graphProperties.Clear();

			var workedDays = weeks.Select(w => (double)w.WorkedDays).ToArray();

			formsPlot.Plot.Clear();
			formsPlot.Plot.Add.Bars(workedDays);
			formsPlot.Plot.Title("Worked Days per Week");
			formsPlot.Plot.XLabel("Week Number");
			formsPlot.Plot.YLabel("Worked Days");
			formsPlot.Refresh();
			
			AdditionalSupport = AdditionalSupport.LinearRegression
				| AdditionalSupport.RollingAverage
				| AdditionalSupport.Distribution;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = "Worked Days",
				Type = PlotIndexType.Data,
				BucketSize = 1d
			});
			graphProperties["Worked Days"] = new GraphProperties(GraphPropertyType.Numeric, workedDays, 1d);
		}

		private void DaysByType(FormsPlot formsPlot, MeijerDayType dayType)
		{
			indexMappings.Clear();
			graphProperties.Clear();

			var seenDaysOfType = 0;
			var date = DateTime.Parse("9/17/2013");
			var dayCountOnDay = new Dictionary<DateTime, double>();

			foreach (var day in days)
			{
				if (day.DayType == dayType)
				{
					seenDaysOfType += 1;
				}
				dayCountOnDay.Add(date, seenDaysOfType);
				date = date.AddDays(1d);
			}

			formsPlot.Plot.Clear();

			formsPlot.Plot.Add.Scatter(dayCountOnDay.Keys.OrderBy(d => d).ToArray(),
				dayCountOnDay.Values.OrderBy(d => d).ToArray());
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title($"{dayType} Days over Time");
			formsPlot.Refresh();
		}

		private void StartTimeByDay(FormsPlot formsPlot)
		{
			indexMappings.Clear();
			graphProperties.Clear();

			var startTimes = new List<KeyValuePair<DateTime, double>>();

			foreach (var day in days)
			{
				double secondsAfterMidnight = double.NaN;
				if (day.ShiftStart.HasValue)
				{
					secondsAfterMidnight = Helpers.GetSecondsAfterMidnight(day.Date, day.ShiftStart.Value);
				}
				startTimes.Add(new KeyValuePair<DateTime, double>(day.Date.ToDateTime(TimeOnly.MinValue), secondsAfterMidnight));
			}
			
			formsPlot.Plot.Clear();
			
			formsPlot.Plot.Add.Scatter(startTimes.Select(t => t.Key).ToArray(),
				startTimes.Select(t => t.Value).ToArray());
			formsPlot.Plot.Axes.DateTimeTicksBottom();
			formsPlot.Plot.Title("Start Times by Day");

			var tickGenerator = new ScottPlot.TickGenerators.NumericAutomatic
			{
				LabelFormatter = CommonTickGenerators.SecondsAfterMidnightFormatter
			};
			formsPlot.Plot.Axes.Left.TickGenerator = tickGenerator;
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.Distribution;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = "Start Times",
				Type = PlotIndexType.Data,
				BucketSize = 900d
			});
			graphProperties["Start Times"] = new GraphProperties(GraphPropertyType.Numeric, startTimes.Select(t => t.Value).ToArray(), 900d);
		}
	}
}
