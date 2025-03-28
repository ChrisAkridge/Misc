using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Logic;
using Celarix.JustForFun.GraphingPlayground.Models;
using Celarix.JustForFun.GraphingPlayground.Models.CSVMaps;
using ScottPlot.TickGenerators;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground.Playgrounds
{
    internal sealed class NoaaClimateDataPlayground : IPlayground
    {
	    private enum TemperatureMode
	    {
		    High,
		    Low,
		    Difference
	    }
	    
		private NoaaClimateData[] rows;
		private Dictionary<string, Action<FormsPlot>> views;
		private readonly List<PlotIndexMapping> indexMappings = [];
		private readonly Dictionary<string, GraphProperties> graphProperties = [];

		public string Name => "NOAA Climate Data";
		public bool NeedsFile => true;
	    public AdditionalSupport AdditionalSupport { get; private set; }
	    public IReadOnlyList<PlotIndexMapping> IndexMappings => indexMappings;
	    public IReadOnlyDictionary<string, GraphProperties> GraphProperties => graphProperties;

	    public void Load(PlaygroundLoadArguments loadArguments)
	    {
		    var csvText = File.ReadAllText(loadArguments.FilePath);
		    var reader = new CSVReader();
		    
		    rows = reader.GetRows<NoaaClimateData, NoaaClimateDataMap>(csvText);
		    FixMissingData();
		    
		    views = new Dictionary<string, Action<FormsPlot>>
			{
				["High Temperature by Day"] = p => TemperatureInfoByDay(p, TemperatureMode.High),
				["Low Temperature by Day"] = p => TemperatureInfoByDay(p, TemperatureMode.Low),
				["Temperature Difference by Day"] = p => TemperatureInfoByDay(p, TemperatureMode.Difference),
				["Days at or over 0\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 0),
				["Days at or over 32\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 32),
				["Days at or over 50\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 50),
				["Days at or over 70\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 70),
				["Days at or over 80\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 80),
				["Days at or over 90\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 90),
				["Days at or over 100\u00b0F per year"] = p => DaysWithHighAtOrOverTemperatureByYear(p, 100),
				["First day at or over 0\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 0),
				["First day at or over 32\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 32),
				["First day at or over 50\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 50),
				["First day at or over 70\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 70),
				["First day at or over 80\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 80),
				["First day at or over 90\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 90),
				["First day at or over 100\u00b0F"] = p => FirstDayAtOrOverTemperature(p, 100),
				["First day after summer at or below 100\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 100),
				["First day after summer at or below 90\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 90),
				["First day after summer at or below 80\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 80),
				["First day after summer at or below 70\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 70),
				["First day after summer at or below 50\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 50),
				["First day after summer at or below 32\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 32),
				["First day after summer at or below 0\u00b0F"] = p => FirstDayAfterSummerSolsticeAtOrBelowTemperature(p, 0)
			};
		}

	    public Action<FormsPlot> GetView(string actionName) => views[actionName];

	    public string[] GetViewNames() => views.Keys.ToArray();

	    private void TemperatureInfoByDay(FormsPlot formsPlot, TemperatureMode mode)
	    {
		    indexMappings.Clear();
		    graphProperties.Clear();
		    
		    var firstDay = DateTime.Parse(rows.First().Date);

			var dates = rows
				.Select(r =>
			    {
				    var date = DateTime.Parse(r.Date);
					return (date - firstDay).Days;
				})
				.ToArray();
		    var temps = rows
			    .Select(r =>
			    {
				    switch (mode)
				    {
					    case TemperatureMode.High:
						    return int.Parse(r.TMax);
					    case TemperatureMode.Low:
						    return int.Parse(r.TMin);
					    case TemperatureMode.Difference:
					    {
						    var tmax = int.Parse(r.TMax);
						    var tmin = int.Parse(r.TMin);

						    return tmax - tmin;
					    }
					    default:
						    throw new ArgumentOutOfRangeException(nameof(mode), $"Invalid mode {mode}");
				    }
			    })
			    .ToArray();

		    formsPlot.Plot.Clear();
		    formsPlot.Plot.Add.Scatter(dates, temps);
		    formsPlot.Plot.Axes.Bottom.TickGenerator = new NumericAutomatic
		    {
				LabelFormatter = CommonTickGenerators.MakeDateTimeTickGeneratorFromBaseDate(firstDay)
		    };
		    formsPlot.Refresh();

		    var name = mode switch
		    {
			    TemperatureMode.High => "High Temperatures by Day",
			    TemperatureMode.Low => "Low Temperatures by Day",
			    TemperatureMode.Difference => "High - Low Temperatures by Day",
			    _ => throw new ArgumentOutOfRangeException(nameof(mode), $"Invalid mode {mode}")
			};
		    AdditionalSupport = AdditionalSupport.LinearRegression
			    | AdditionalSupport.Distribution;
		    indexMappings.Add(new PlotIndexMapping
		    {
			    Index = 0,
			    Name = name,
			    Type = PlotIndexType.Data,
			    BucketSize = 10d
			});
		    graphProperties[name] = new GraphProperties(GraphPropertyType.Numeric, temps, 5d);
		}

	    private void DaysWithHighAtOrOverTemperatureByYear(FormsPlot formsPlot, int temperature)
	    {
			indexMappings.Clear();
			graphProperties.Clear();

			var counts = new Dictionary<int, int>();

			foreach (var row in rows)
			{
				var date = DateTime.Parse(row.Date);
				var year = date.Year;

				if (year == DateTime.Now.Year)
				{
					// Skip the current year, it's not complete
					continue;
				}

				counts.TryAdd(year, 0);

				var tmax = int.Parse(row.TMax);
				if (tmax >= temperature)
				{
					counts[year]++;
				}
			}
			
			formsPlot.Plot.Clear();
			var valuesAsDoubles = counts.Values.Select(c => (double)c).ToArray();
			formsPlot.Plot.Add.Scatter(counts.Keys.ToArray(), counts.Values.ToArray());
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression;
			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = $"Days at or over {temperature}\u00b0F per year",
				Type = PlotIndexType.Data,
				BucketSize = 1d
			});
			graphProperties["Days"] = new GraphProperties(GraphPropertyType.Numeric, valuesAsDoubles, 1d);
		}

	    private void FirstDayAtOrOverTemperature(FormsPlot formsPlot, int temperature)
	    {
		    indexMappings.Clear();
		    graphProperties.Clear();
		    
		    var firstDays = new Dictionary<int, int>();

		    foreach (var year in rows.GroupBy(r => DateTime.Parse(r.Date).Year))
		    {
				if (year.Key == DateTime.Now.Year)
				{
					// Skip the current year, it's not complete
					continue;
				}

				var dayNumber = 1;

			    foreach (var dayTMax in year.Select(day => int.Parse(day.TMax)))
			    {
				    if (dayTMax >= temperature)
				    {
					    firstDays.TryAdd(year.Key, dayNumber);
					    break;
				    }

				    dayNumber += 1;
			    }
		    }
		    
		    formsPlot.Plot.Clear();
		    var valuesAsDoubles = firstDays.Values.Select(v => (double)v).ToArray();
		    formsPlot.Plot.Add.Scatter(firstDays.Keys.ToArray(), firstDays.Values.ToArray());
		    formsPlot.Plot.Axes.Left.TickGenerator = new NumericAutomatic
		    {
			    LabelFormatter = CommonTickGenerators.MonthAndDayFromDayNumber
		    };
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression;
			indexMappings.Add(new PlotIndexMapping
		    {
			    Index = 0,
			    Name = $"First day of year at or over {temperature}\u00b0F",
			    Type = PlotIndexType.Data,
			    BucketSize = 1d
			});
		    graphProperties["First Day"] = new GraphProperties(GraphPropertyType.Numeric, valuesAsDoubles, 1d);
	    }

	    private void FirstDayAfterSummerSolsticeAtOrBelowTemperature(FormsPlot formsPlot, int temperature)
	    {
		    indexMappings.Clear();
		    graphProperties.Clear();

		    var firstDays = new Dictionary<int, int>();

		    foreach (var year in rows.GroupBy(r => DateTime.Parse(r.Date).Year))
		    {
			    if (year.Key == DateTime.Now.Year)
				{
					// Skip the current year, it's not complete
					continue;
				}

			    var solsticeDayNumberForYear = Helpers.GetSummerSolsticeDayForYear(year.Key);
				foreach (var day in year)
			    {
				    var date = DateTime.Parse(day.Date);
				    var dayNumber = date.DayOfYear;
				    if (dayNumber < solsticeDayNumberForYear)
					{
						continue;
					}
				    
				    var dayTMax = int.Parse(day.TMax);
				    if (dayTMax <= temperature)
					{
						firstDays.TryAdd(year.Key, dayNumber);
						break;
					}
				}
		    }

			formsPlot.Plot.Clear();
			var valuesAsDoubles = firstDays.Values.Select(v => (double)v).ToArray();
			formsPlot.Plot.Add.Scatter(firstDays.Keys.ToArray(), firstDays.Values.ToArray());

			formsPlot.Plot.Axes.Left.TickGenerator = new NumericAutomatic
			{
				LabelFormatter = CommonTickGenerators.MonthAndDayFromDayNumber
			};
			formsPlot.Refresh();

			AdditionalSupport = AdditionalSupport.LinearRegression;

			indexMappings.Add(new PlotIndexMapping
			{
				Index = 0,
				Name = $"First day of year after summer solstice at or below {temperature}\u00b0F",
				Type = PlotIndexType.Data,
				BucketSize = 1d
			});
			graphProperties["First Day"] = new GraphProperties(GraphPropertyType.Numeric, valuesAsDoubles, 1d);
		}

	    private void FixMissingData()
	    {
		    var lastTMax = int.MinValue.ToString();
		    var lastTMin = int.MinValue.ToString();

		    for (var i = 0; i < rows.Length; i++)
		    {
				var row = rows[i];

				if (!int.TryParse(row.TMax, out var _))
				{
					rows[i].TMax = lastTMax;
				}

				if (!int.TryParse(row.TMin, out var _))
				{
					rows[i].TMin = lastTMin;
				}

				lastTMax = rows[i].TMax;
				lastTMin = rows[i].TMin;
			}
		}
    }
}
