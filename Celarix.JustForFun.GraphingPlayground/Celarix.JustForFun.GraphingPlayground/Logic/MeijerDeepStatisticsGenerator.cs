using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Collections;
using Celarix.JustForFun.GraphingPlayground.Models;
using DocumentFormat.OpenXml.Drawing;
using ScottPlot;
using ScottPlot.Palettes;
using ScottPlot.TickGenerators;
using Color = ScottPlot.Color;
using Path = System.IO.Path;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal sealed class MeijerDeepStatisticsGenerator
	{
		private readonly string savePath;

		private readonly MeijerDay[] days;
		private readonly MeijerWeek[] weeks;
		private readonly MeijerSeries[] series;

		public MeijerDeepStatisticsGenerator(string savePath)
		{
			this.savePath = savePath;
			MeijerStatisticsProvider provider = new();
			
			days = provider.GetDays().ToArray();
			weeks = provider.GetWeeks(days).ToArray();
			series = provider.GetSeries(days).ToArray();
		}

		public void Generate()
		{
			(string Name, string Extension, Action<string> Generator)[] actions =
			[
				//(nameof(TotalDuration), ".txt", TotalDuration),
				//(nameof(TotalDaysByType), ".png", TotalDaysByType),
				//(nameof(TotalDaysByWorkingOrOff), ".png", TotalDaysByWorkingOrOff),
				//(nameof(DayTypesByDay), ".html", DayTypesByDay),
				//(nameof(ConglomerateShiftByDayType), ".txt", ConglomerateShiftByDayType),
				//(nameof(WorkingVsNonWorkingDaysByCalendarDate), ".png", WorkingVsNonWorkingDaysByCalendarDate),
				//(nameof(WorkingVsNonWorkingDaysByDayOfMonth), ".png", WorkingVsNonWorkingDaysByDayOfMonth),
				//(nameof(WorkingVsNonWorkingDaysByWeekday), ".png", WorkingVsNonWorkingDaysByWeekday),
				//(nameof(WorkingVsNonWorkingDaysByWeekdayOrWeekend), ".png", WorkingVsNonWorkingDaysByWeekdayOrWeekend),
				//(nameof(MostAndLeastLikelyDayOfYearForEachDayType), ".txt", MostAndLeastLikelyDayOfMonthForEachDayType),
				//(nameof(MostAndLeastLikelyDayOfMonthForEachDayType), ".txt", MostAndLeastLikelyDayOfMonthForEachDayType),
				//(nameof(MostAndLeastLikelyDayOfWeekForEachDayType), ".txt", MostAndLeastLikelyDayOfWeekForEachDayType),
				//(nameof(MostAndLeastLikelyWeekdayOrWeekendDayForEachDayType), ".txt", MostAndLeastLikelyWeekdayOrWeekendDayForEachDayType),
				//(nameof(DayTypeAccumulatingTotals), ".png", DayTypeAccumulatingTotals),
				//(nameof(WeekTypeDistribution), ".png", WeekTypeDistribution),
				//(nameof(ShortVsLongWeekDistribution), ".png", ShortVsLongWeekDistribution),
				//(nameof(LongestStretchOfWeekType), ".txt", LongestStretchOfWeekType),
				//(nameof(LongestGapBetweenWeekTypes), ".txt", LongestGapBetweenWeekTypes),
				//(nameof(WeekTypesByDay), ".html", WeekTypesByDay),
				//(nameof(WeekTypesByWeekOfYear), ".txt", WeekTypesByWeekOfYear),
				//(nameof(SeriesCountsByType), ".png", SeriesCountsByType),
				//(nameof(SeriesDistributionsByType), ".png", SeriesDistributionsByType),
				//(nameof(TotalDaysSpentInEachSeriesByType), ".txt", TotalDaysSpentInEachSeriesByType),
				//(nameof(SeriesByTypeAndStartingWeekday), ".png", SeriesByTypeAndStartingWeekday),
				//(nameof(SeriesByTypeAndEndingWeekday), ".png", SeriesByTypeAndEndingWeekday),
				//(nameof(SeriesByTypeLengthAndStartingWeekday), ".png", SeriesByTypeLengthAndStartingWeekday),
				//(nameof(SeriesByTypeLengthAndEndingWeekday), ".png", SeriesByTypeLengthAndEndingWeekday),
				//(nameof(WorkingSeriesLengthsByDay), ".html", WorkingSeriesLengthsByDay),
				//(nameof(ExpectedDaysUntilNextDayOff), ".png", ExpectedDaysUntilNextDayOff),
				//(nameof(LongestStretchOfNDaySeriesOfType), ".txt", LongestStretchOfNDaySeriesOfType),
				//(nameof(LongestGapBetweenNDaySeriesOfType), ".txt", LongestGapBetweenNDaySeriesOfType),
				//(nameof(WorkingVsNonWorkingSeries), ".txt", WorkingVsNonWorkingSeries),
				(nameof(LongestStretchOfNonWorkingSeriesOfAtLeastNDays), ".png", LongestStretchOfNonWorkingSeriesOfAtLeastNDays)
			];

			for (var i = 0; i < actions.Length; i++)
			{
				var action = actions[i];
				var filePath = Path.Combine(savePath, $"{i}_{action.Name}{action.Extension}");
				action.Generator(filePath);
			}
		}

		private static StreamWriter OpenFile(string filePath) => new(filePath);

		private void TotalDuration(string filePath)
		{
			const string question =
				"From 2013-09-17T12:00:00-04:00 to 2018-05-19T22:00:00-04:00, what was the total duration of this period? In years, months, weeks, days, hours, minutes, seconds?";

			using var writer = OpenFile(filePath);
			var seniorityStart = DateTimeOffset.Parse("2013-09-17T12:00:00-04:00");
			var seniorityEnd = DateTimeOffset.Parse("2018-05-19T22:00:00-04:00");
			var period = seniorityEnd - seniorityStart;
			
			writer.WriteLine(question);
			writer.WriteLine();
			writer.WriteLine($"The period is {period}.");
			writer.WriteLine();
			writer.WriteLine($"Total years: {period.TotalDays / 365.2524d}");
			writer.WriteLine($"Total months: {period.TotalDays / 30.436875d}");
			writer.WriteLine($"Total weeks: {period.TotalDays / 7d}");
			writer.WriteLine($"Total days: {period.TotalDays}");
			writer.WriteLine($"Total hours: {period.TotalHours}");
			writer.WriteLine($"Total minutes: {period.TotalMinutes}");
			writer.WriteLine($"Total seconds: {period.TotalSeconds}");
			writer.Close();
		}

		private void TotalDaysByType(string filePath)
		{
			const string question = "What was the total number of working, non-working, PTO, NPTO, and other days?";

			var workingDayCount = days.Count(d => d.DayType == MeijerDayType.Working);
			var nonWorkingDayCount = days.Count(d => d.DayType == MeijerDayType.NonWorking);
			var paidTimeOffDayCount = days.Count(d => d.DayType == MeijerDayType.PaidTimeOff);
			var nonPaidTimeOffDayCount = days.Count(d => d.DayType == MeijerDayType.NonPaidTimeOff);
			var otherDayCount = days.Count(d => d.DayType == MeijerDayType.Other);
			
			var plot = new Plot();

			var workingBar = plot.Add.Bar(position: 1, value: workingDayCount);
			var nonWorkingBar = plot.Add.Bar(position: 2, value: nonWorkingDayCount);
			var ptoBar = plot.Add.Bar(position: 3, value: paidTimeOffDayCount);
			var nptoBar = plot.Add.Bar(position: 4, value: nonPaidTimeOffDayCount);
			var otherBar = plot.Add.Bar(position: 5, value: otherDayCount);

			workingBar.LegendText = workingDayCount.ToString();
			nonWorkingBar.LegendText = nonWorkingDayCount.ToString();
			ptoBar.LegendText = paidTimeOffDayCount.ToString();
			nptoBar.LegendText = nonPaidTimeOffDayCount.ToString();
			otherBar.LegendText = otherDayCount.ToString();

			var ticks = new Tick[]
			{
				new(1, "Working"),
				new(2, "Non-Working"),
				new(3, "PTO"),
				new(4, "NPTO"),
				new(5, "Other")
			};

			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.Axes.Bottom.MajorTickStyle.Length = 0;
			plot.HideGrid();
			
			plot.Axes.Margins(bottom: 0);
			plot.ShowLegend();
			plot.Title(question, size: 10f);
			
			plot.SavePng(filePath, 960, 540);
		}

		private void TotalDaysByWorkingOrOff(string filePath)
		{
			const string question = "What was the total number of working and off days?";

			var workingDayCount = days.Count(d => d.DayType is MeijerDayType.Working or MeijerDayType.Other);
			var nonWorkingDayCount = days.Length - workingDayCount;
			
			var plot = new Plot();
			
			var workingBar = plot.Add.Bar(position: 1, value: workingDayCount);
			var offDayBar = plot.Add.Bar(position: 2, value: nonWorkingDayCount);
			
			workingBar.LegendText = workingDayCount.ToString();
			offDayBar.LegendText = nonWorkingDayCount.ToString();
			
			var ticks = new Tick[]
			{
				new(1, "Working"),
				new(2, "Off")
			};
			
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.Axes.Bottom.MajorTickStyle.Length = 0;
			plot.HideGrid();
			
			plot.Axes.Margins(bottom: 0);
			plot.ShowLegend();
			plot.Title(question, size: 10f);
			
			plot.SavePng(filePath, 960, 540);
		}

		private void DayTypesByDay(string filePath)
		{
			const string question = "Over all days, what was the type of each day?";

			using var writer = OpenFile(filePath);

			const string workingDayStyle = "background-color: yellow;";
			const string nonWorkingDayStyle = "background-color: green;";
			const string paidTimeOffDayStyle = "background-color: blue;";
			const string nonPaidTimeOffDayStyle = "background-color: darkblue; color: white;";
			const string otherDayStyle = "background-color: darkorange;";

			var dayStyles = days.ToDictionary(d => d.Date,
				d => d.DayType switch
				{
					MeijerDayType.Working => workingDayStyle,
					MeijerDayType.NonWorking => nonWorkingDayStyle,
					MeijerDayType.PaidTimeOff => paidTimeOffDayStyle,
					MeijerDayType.NonPaidTimeOff => nonPaidTimeOffDayStyle,
					MeijerDayType.Other => otherDayStyle,
					_ => ""
				});

			var legend = new Dictionary<string, string>
			{
				["Working Day"] = workingDayStyle,
				["Non-Working Day"] = nonWorkingDayStyle,
				["Paid Time Off"] = paidTimeOffDayStyle,
				["Non-Paid Time Off"] = nonPaidTimeOffDayStyle,
				["Other"] = otherDayStyle
			};

			var html = HtmlCalendarBuilder.BuildHtml(question, dayStyles, legend);
			writer.Write(html);
			writer.Close();
		}

		private void ConglomerateShiftByDayType(string filePath)
		{
			const string question =
				"If each day type was grouped together, how long into my seniority would the working days stretch?";

			using var writer = OpenFile(filePath);

			var workingDayCount = days.Count(d => d.DayType == MeijerDayType.Working);
			var nonWorkingDayCount = days.Count(d => d.DayType == MeijerDayType.NonWorking);
			var paidTimeOffDayCount = days.Count(d => d.DayType == MeijerDayType.PaidTimeOff);
			var nonPaidTimeOffDayCount = days.Count(d => d.DayType == MeijerDayType.NonPaidTimeOff);
			var otherDayCount = days.Count(d => d.DayType == MeijerDayType.Other);

			var seniorityStart = DateOnly.Parse("2013-09-17");
			var workingDaysGoTo = seniorityStart.AddDays(workingDayCount);
			var nonWorkingDaysGoTo = seniorityStart.AddDays(nonWorkingDayCount);
			var paidTimeOffDaysGoTo = seniorityStart.AddDays(paidTimeOffDayCount);
			var nonPaidTimeOffDaysGoTo = seniorityStart.AddDays(nonPaidTimeOffDayCount);
			var otherDaysGoTo = seniorityStart.AddDays(otherDayCount);
			
			writer.WriteLine(question);
			writer.WriteLine();
			writer.WriteLine($"If all working days were put together, they'd stretch until {workingDaysGoTo:dddd, MMMM d, yyyy}.");
			writer.WriteLine($"If all non-working days were put together, they'd stretch until {nonWorkingDaysGoTo:dddd, MMMM d, yyyy}.");
			writer.WriteLine($"If all paid time off days were put together, they'd stretch until {paidTimeOffDaysGoTo:dddd, MMMM d, yyyy}.");
			writer.WriteLine($"If all non-paid time off days were put together, they'd stretch until {nonPaidTimeOffDaysGoTo:dddd, MMMM d, yyyy}.");
			writer.WriteLine($"If all other days were put together, they'd stretch until {otherDaysGoTo:dddd, MMMM d, yyyy}.");
			writer.Close();
		}

		private void WorkingVsNonWorkingDaysByCalendarDate(string filePath)
		{
			const string question = "How many working vs. non-working days of the year?";

			var workingAndOffDaysByDate = days
				.GroupBy(d => new
				{
					d.Date.Month,
					d.Date.Day
				})
				.Select(g => new
				{
					g.Key.Month,
					g.Key.Day,
					WorkingDays = g.Count(d => d.DayType is MeijerDayType.Working or MeijerDayType.Other),
					NonWorkingDays = g.Count(d => d.DayType is MeijerDayType.NonWorking or MeijerDayType.PaidTimeOff or MeijerDayType.NonPaidTimeOff)
				})
				.OrderBy(a => a.Month)
				.ThenBy(a => a.Day)
				.ToArray();

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();

			for (var i = 0; i < 366; i++)
			{
				var date = workingAndOffDaysByDate[i];
				
				bars.Add(new Bar
				{
					Position = i + 1,
					ValueBase = 0d,
					Value = date.WorkingDays,
					FillColor = palette.GetColor(0)
				});
				
				bars.Add(new Bar
				{
					Position = i + 1,
					ValueBase = date.WorkingDays,
					Value = date.WorkingDays + date.NonWorkingDays,
					FillColor = palette.GetColor(1)
				});
			}

			var barPlot = plot.Add.Bars(bars);
			barPlot.Horizontal = true;

			var ticks = Enumerable.Range(1, 366)
				.Select(n => new Tick(n, CommonTickGenerators.MonthAndDayFromDayNumber(n)))
				.ToArray();

			plot.Axes.Left.TickGenerator = new NumericManual(ticks);
			plot.Axes.Left.MajorTickStyle.Length = 0;
			plot.HideGrid();
			plot.Axes.Margins(left: 0d);
			plot.Axes.SetLimits(0d, 6d, 370d, -4d);
			plot.Title(question, size: 14f);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.LowerRight;
			plot.Legend.ManualItems.Add(new() { LabelText = "Working", FillColor = palette.GetColor(0) });
			plot.Legend.ManualItems.Add(new() { LabelText = "Non-Working", FillColor = palette.GetColor(1) });

			plot.SavePng(filePath, 1024, 4096);
		}

		private void WorkingVsNonWorkingDaysByDayOfMonth(string filePath)
		{
			const string question = "How many working vs. non-working days of the month?";

			var workingAndOffDaysByDate = days
				.GroupBy(d => new
				{
					d.Date.Day
				})
				.Select(g => new
				{
					g.Key.Day,
					WorkingDays = g.Count(d => d.DayType is MeijerDayType.Working or MeijerDayType.Other),
					NonWorkingDays = g.Count(d =>
						d.DayType is MeijerDayType.NonWorking or MeijerDayType.PaidTimeOff
							or MeijerDayType.NonPaidTimeOff)
				})
				.OrderBy(a => a.Day)
				.ToArray();

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();

			for (var i = 0; i < 31; i++)
			{
				var date = workingAndOffDaysByDate[i];

				bars.Add(new Bar
				{
					Position = i + 1,
					ValueBase = 0d,
					Value = date.WorkingDays,
					FillColor = palette.GetColor(0),
					Label = date.WorkingDays.ToString(),
					CenterLabel = true
				});

				bars.Add(new Bar
				{
					Position = i + 1,
					ValueBase = date.WorkingDays,
					Value = date.WorkingDays + date.NonWorkingDays,
					FillColor = palette.GetColor(1),
					Label = date.NonWorkingDays.ToString(),
					CenterLabel = true
				});
			}

			var barPlot = plot.Add.Bars(bars);
			barPlot.Horizontal = true;

			var ticks = Enumerable.Range(1, 31)
				.Select(n => new Tick(n, $"{n}"))
				.ToArray();

			plot.Axes.Left.TickGenerator = new NumericManual(ticks);
			plot.Axes.Left.MajorTickStyle.Length = 0;
			plot.HideGrid();
			plot.Axes.Margins(left: 0d);
			plot.Axes.SetLimits(0d, 60d, 32d, -1d);
			plot.Title(question, size: 14f);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.LowerRight;

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Working",
				FillColor = palette.GetColor(0)
			});

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Non-Working",
				FillColor = palette.GetColor(1)
			});

			plot.SavePng(filePath, 1024, 1024);
		}

		private void WorkingVsNonWorkingDaysByWeekday(string filePath)
		{
			const string question = "How many working vs. non-working weekdays?";

			var workingAndOffDaysByDate = days
				.GroupBy(d => new
				{
					d.Date.DayOfWeek
				})
				.Select(g => new
				{
					g.Key.DayOfWeek,
					WorkingDays = g.Count(d => d.DayType is MeijerDayType.Working or MeijerDayType.Other),
					NonWorkingDays = g.Count(d =>
						d.DayType is MeijerDayType.NonWorking or MeijerDayType.PaidTimeOff
							or MeijerDayType.NonPaidTimeOff)
				})
				.OrderBy(a => a.DayOfWeek)
				.ToArray();

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();

			for (var i = 0; i < 7; i++)
			{
				var date = workingAndOffDaysByDate[i];

				bars.Add(new Bar
				{
					Position = i,
					ValueBase = 0d,
					Value = date.WorkingDays,
					FillColor = palette.GetColor(0),
					Label = date.WorkingDays.ToString(),
					CenterLabel = true
				});

				bars.Add(new Bar
				{
					Position = i,
					ValueBase = date.WorkingDays,
					Value = date.WorkingDays + date.NonWorkingDays,
					FillColor = palette.GetColor(1),
					Label = date.NonWorkingDays.ToString(),
					CenterLabel = true
				});
			}

			var barPlot = plot.Add.Bars(bars);
			barPlot.Horizontal = true;

			var ticks = Enumerable.Range(0, 7)
				.Select(n => new Tick(n, ((DayOfWeek)n).ToString()))
				.ToArray();

			plot.Axes.Left.TickGenerator = new NumericManual(ticks);
			plot.Axes.Left.MajorTickStyle.Length = 0;
			plot.HideGrid();
			plot.Axes.Margins(left: 0d);
			plot.Axes.SetLimits(0d, 280d, 6.5d, -0.5d);
			plot.Title(question, size: 14f);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.LowerRight;

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Working", FillColor = palette.GetColor(0)
			});

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Non-Working", FillColor = palette.GetColor(1)
			});

			plot.SavePng(filePath, 1024, 1024);
		}

		private void WorkingVsNonWorkingDaysByWeekdayOrWeekend(string filePath)
		{
			const string question = "How many working vs. non-working weekday/weekend days?";

			var workingAndOffDaysByDate = days
				.GroupBy(d => d.Date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
				.Select(g => new
				{
					IsWeekday = g.Key,
					WorkingDays = g.Count(d => d.DayType is MeijerDayType.Working or MeijerDayType.Other),
					NonWorkingDays = g.Count(d =>
						d.DayType is MeijerDayType.NonWorking or MeijerDayType.PaidTimeOff
							or MeijerDayType.NonPaidTimeOff)
				})
				.ToArray();

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();

			for (var i = 0; i < 2; i++)
			{
				var date = workingAndOffDaysByDate[i];

				bars.Add(new Bar
				{
					Position = i,
					ValueBase = 0d,
					Value = date.WorkingDays,
					FillColor = palette.GetColor(0),
					Label = date.WorkingDays.ToString(),
					CenterLabel = true
				});

				bars.Add(new Bar
				{
					Position = i,
					ValueBase = date.WorkingDays,
					Value = date.WorkingDays + date.NonWorkingDays,
					FillColor = palette.GetColor(1),
					Label = date.NonWorkingDays.ToString(),
					CenterLabel = true
				});
			}

			var barPlot = plot.Add.Bars(bars);
			barPlot.Horizontal = true;

			var ticks = new Tick[]
			{
				new(0, "Weekday"),
				new(1, "Weekend")
			};

			plot.Axes.Left.TickGenerator = new NumericManual(ticks);
			plot.Axes.Left.MajorTickStyle.Length = 0;
			plot.HideGrid();
			plot.Axes.Margins(left: 0d);
			plot.Axes.SetLimits(0d, 1500d, 2.5d, -0.5d);
			plot.Title(question, size: 14f);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.LowerRight;

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Working", FillColor = palette.GetColor(0)
			});

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Non-Working", FillColor = palette.GetColor(1)
			});

			plot.SavePng(filePath, 1024, 1024);
		}

		private void MostAndLeastLikelyDayOfYearForEachDayType(string filePath)
		{
			const string question = "What was the most likely day of the year I worked? The least likely?";

			using var writer = OpenFile(filePath);

			var daysByDate = days.GroupBy(d => new
			{
				d.Date.Month,
				d.Date.Day,
				d.DayType
			});

			var dayCountsByDateAndType = daysByDate
				.Select(g => new
				{
					g.Key.Month,
					g.Key.Day,
					g.Key.DayType,
					Count = g.Count()
				})
				.GroupBy(c => c.DayType)
				.OrderBy(g => g.Key);
			
			writer.WriteLine(question);
			writer.WriteLine();

			foreach (var grouping in dayCountsByDateAndType)
			{
				writer.WriteLine($"{grouping.Key}:");

				foreach (var date in grouping.OrderByDescending(c => c.Count))
				{
					writer.WriteLine($"    {date.Count} occurrences: {DateTimeFormatInfo.CurrentInfo.GetMonthName(date.Month)} {date.Day}");
				}
				
				writer.WriteLine();
			}
			
			writer.Close();
		}

		private void MostAndLeastLikelyDayOfMonthForEachDayType(string filePath)
		{
			const string question = "What was the most likely day of the month I worked? The least likely?";

			using var writer = OpenFile(filePath);

			var daysByDate = days.GroupBy(d => new
			{
				d.Date.Day,
				d.DayType
			});

			var dayCountsByDateAndType = daysByDate
				.Select(g => new
				{
					g.Key.Day,
					g.Key.DayType,
					Count = g.Count()
				})
				.GroupBy(c => c.DayType)
				.OrderBy(g => g.Key);

			writer.WriteLine(question);
			writer.WriteLine();

			foreach (var grouping in dayCountsByDateAndType)
			{
				writer.WriteLine($"{grouping.Key}:");

				foreach (var date in grouping.OrderByDescending(c => c.Count))
				{
					writer.WriteLine($"    {date.Count} occurrences: the {date.Day.Ordinal()}");
				}

				writer.WriteLine();
			}

			writer.Close();
		}

		private void MostAndLeastLikelyDayOfWeekForEachDayType(string filePath)
		{
			const string question = "What was the most likely day of the week I worked? The least likely?";

			using var writer = OpenFile(filePath);

			var daysByDate = days.GroupBy(d => new
			{
				d.Date.DayOfWeek,
				d.DayType
			});

			var dayCountsByDateAndType = daysByDate
				.Select(g => new
				{
					g.Key.DayOfWeek,
					g.Key.DayType,
					Count = g.Count()
				})
				.GroupBy(c => c.DayType)
				.OrderBy(g => g.Key);

			writer.WriteLine(question);
			writer.WriteLine();

			foreach (var grouping in dayCountsByDateAndType)
			{
				writer.WriteLine($"{grouping.Key}:");

				foreach (var date in grouping.OrderByDescending(c => c.Count))
				{
					writer.WriteLine($"    {date.Count} occurrences: {date.DayOfWeek}");
				}

				writer.WriteLine();
			}

			writer.Close();
		}

		private void MostAndLeastLikelyWeekdayOrWeekendDayForEachDayType(string filePath)
		{
			const string question = "What was the most likely weekday/weekend day I worked? The least likely?";

			using var writer = OpenFile(filePath);

			var daysByDate = days.GroupBy(d => new
			{
				IsWeekday = d.Date.DayOfWeek != DayOfWeek.Saturday && d.Date.DayOfWeek != DayOfWeek.Sunday,
				d.DayType
			});

			var dayCountsByDateAndType = daysByDate
				.Select(g => new
				{
					g.Key.IsWeekday,
					g.Key.DayType,
					Count = g.Count()
				})
				.GroupBy(c => c.DayType)
				.OrderBy(g => g.Key);

			writer.WriteLine(question);
			writer.WriteLine();

			foreach (var grouping in dayCountsByDateAndType)
			{
				writer.WriteLine($"{grouping.Key}:");

				foreach (var date in grouping.OrderByDescending(c => c.Count))
				{
					writer.WriteLine($"    {date.Count} occurrences: {(date.IsWeekday ? "Weekday" : "Weekend")}");
				}

				writer.WriteLine();
			}

			writer.Close();
		}

		private void DayTypeAccumulatingTotals(string filePath)
		{
			const string question =
				"What was the accumulating total of each day type? Are there discontinuities in the growth rate?";

			var currentWorkingDayCount = 0d;
			var currentNonWorkingDayCount = 0d;
			var currentPaidTimeOffDayCount = 0d;
			var currentNonPaidTimeOffDayCount = 0d;
			var currentOtherDayCount = 0d;

			var workingDayCounts = new List<double>(days.Length);
			var nonWorkingDayCounts = new List<double>(days.Length);
			var paidTimeOffDayCounts = new List<double>(days.Length);
			var nonPaidTimeOffDayCounts = new List<double>(days.Length);
			var otherDayCounts = new List<double>(days.Length);

			foreach (var day in days)
			{
				switch (day.DayType)
				{
					case MeijerDayType.Working:
						currentWorkingDayCount = day.DayNumberOfType;

						break;
					case MeijerDayType.NonWorking:
						currentNonWorkingDayCount = day.DayNumberOfType;

						break;
					case MeijerDayType.PaidTimeOff:
						currentPaidTimeOffDayCount = day.DayNumberOfType;

						break;
					case MeijerDayType.NonPaidTimeOff:
						currentNonPaidTimeOffDayCount = day.DayNumberOfType;

						break;
					case MeijerDayType.Other:
						currentOtherDayCount = day.DayNumberOfType;

						break;
					default:
						throw new InvalidOperationException();
				}
				
				workingDayCounts.Add(currentWorkingDayCount);
				nonWorkingDayCounts.Add(currentNonWorkingDayCount);
				paidTimeOffDayCounts.Add(currentPaidTimeOffDayCount);
				nonPaidTimeOffDayCounts.Add(currentNonPaidTimeOffDayCount);
				otherDayCounts.Add(currentOtherDayCount);
			}

			var plot = new Plot();

			var yellow = Color.FromARGB(0xFFFFFF00u);
			var green = Color.FromARGB(0xFF008000u);
			var blue = Color.FromARGB(0xFF0000FFu);
			var darkBlue = Color.FromARGB(0xFF00008Bu);
			var darkOrange = Color.FromARGB(0xFFFF8C00u);
			
			plot.Add.Signal(workingDayCounts, color: yellow);
			plot.Add.Signal(nonWorkingDayCounts, color: green);
			plot.Add.Signal(paidTimeOffDayCounts, color: blue);
			plot.Add.Signal(nonPaidTimeOffDayCounts, color: darkBlue);
			plot.Add.Signal(otherDayCounts, color: darkOrange);
			
			plot.Title(question, size: 14f);
			plot.Axes.SetLimits(0d, 1750d, 0d, 1050d);

			var tickGenerator = new NumericAutomatic
			{
				LabelFormatter = CommonTickGenerators.DayOfSeniorityFromDayNumber
			};
			plot.Axes.Bottom.TickGenerator = tickGenerator;

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperLeft;

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Working", FillColor = yellow
			});

			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Non-Working", FillColor = green
			});
			
			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Paid Time Off", FillColor = blue
			});
			
			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Non-Paid Time Off", FillColor = darkBlue
			});
			
			plot.Legend.ManualItems.Add(new()
			{
				LabelText = "Other", FillColor = darkOrange
			});

			plot.SavePng(filePath, 3840, 2160);
		}

		private void WeekTypeDistribution(string filePath)
		{
			const string question = "Given week types of 0-7, 1-6, etc., what is the distribution of those types?";

			var weeksByWeekType = weeks
				.GroupBy(w => w.WorkedDays)
				.Select(g => new
				{
					WorkedDays = g.Key,
					WeekType = $"{g.Key}-{7 - g.Key}",
					Count = g.Count()
				})
				.OrderBy(t => t.WorkedDays);

			var plot = new Plot();

			var bars = weeksByWeekType.Select((t, i) => new Bar
			{
				Position = i,
				Value = t.Count,
				FillColor = Color.FromARGB(0xFF0000FFu),
				Label = $"{t.Count} ({t.WorkedDays * t.Count} working, {(7 - t.WorkedDays) * t.Count} off)"
			});

			plot.Add.Bars(bars);

			var ticks = Enumerable.Range(0, 7)
				.Select(t => new Tick(t, $"{t}-{7 - t}"))
				.ToArray();
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, size: 14f);
			
			plot.SavePng(filePath, 1024, 1024);
		}

		private void ShortVsLongWeekDistribution(string filePath)
		{
			const string question = "What is the distribution of week 5-2 and greater vs. weeks 4-3 and shorter?";

			var weeksByWeekType = weeks
				.GroupBy(w => w.WorkedDays >= 5)
				.Select(g => new
				{
					g.First().WorkedDays,
					IsLong = g.Key,
					WeekType = (g.Key) ? "Long" : "Short",
					Count = g.Count()
				})
				.OrderBy(t => t.IsLong ? 1 : 0);

			var plot = new Plot();

			var bars = weeksByWeekType.Select((t, i) => new Bar
			{
				Position = i,
				Value = t.Count,
				FillColor = Color.FromARGB(0xFF0000FFu),
				Label = $"{t.Count}"
			});

			plot.Add.Bars(bars);

			var ticks = new Tick[]
			{
				new(0, "Short"),
				new(1, "Long")
			};
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, size: 14f);

			plot.SavePng(filePath, 1024, 1024);
		}

		private void LongestStretchOfWeekType(string filePath)
		{
			const string question = "For each week type, what was their longest stretch?";

			var writer = OpenFile(filePath);
			
			writer.WriteLine(question);
			writer.WriteLine();

			var longestWeekStretches = new int[7];
			var lastWeekWorkedDays = weeks[0].WorkedDays;
			var currentWeekStretch = 0;

			foreach (var workedDays in weeks.Skip(1).Select(week => week.WorkedDays))
			{
				if (workedDays == lastWeekWorkedDays)
				{
					currentWeekStretch++;
				}
				else
				{
					if (currentWeekStretch > longestWeekStretches[lastWeekWorkedDays])
					{
						longestWeekStretches[lastWeekWorkedDays] = currentWeekStretch;
					}

					currentWeekStretch = 1;
					lastWeekWorkedDays = workedDays;
				}
			}

			for (int i = 0; i < 7; i++)
			{
				var weekType = $"{i}-{7 - i}";
				var longestStretch = longestWeekStretches[i];
				var longestStretchInDays = longestStretch * 7;
				var longestStretchWorkedDays = longestStretch * i;
				var longestStretchOffDays = longestStretchInDays - longestStretchWorkedDays;
				writer.WriteLine($"- Longest stretch of {weekType} weeks: {longestStretch} weeks ({longestStretchInDays} days, {longestStretchWorkedDays} worked, {longestStretchOffDays} off)");
			}
			
			writer.Close();
		}

		private void LongestGapBetweenWeekTypes(string filePath)
		{
			const string question = "For each week type, what was the longest gap between two of them?";
			
			var writer = OpenFile(filePath);
			
			writer.WriteLine(question);
			writer.WriteLine();
			
			var longestWeekGaps = new int[7];
			var currentWeekGaps = new int[7];

			foreach (var workedDays in weeks.Skip(1).Select(w => w.WorkedDays))
			{
				for (var i = 0; i < 7; i++)
				{
					if (i == workedDays) { continue; }

					currentWeekGaps[i] += 1;
				}

				if (currentWeekGaps[workedDays] > longestWeekGaps[workedDays])
				{
					longestWeekGaps[workedDays] = currentWeekGaps[workedDays];
				}

				currentWeekGaps[workedDays] = 0;
			}
			
			for (int i = 0; i < 7; i++)
			{
				var weekType = $"{i}-{7 - i}";
				var longestGap = longestWeekGaps[i];
				writer.WriteLine($"- Longest gap between {weekType} weeks: {longestGap} weeks ({longestGap * 7} days)");
			}
			
			writer.Close();
		}

		private void WeekTypesByDay(string filePath)
		{
			const string question = "Over all days, what was the type of each week?";

			using var writer = OpenFile(filePath);

			const string zeroAndSevenStyle = "background-color: lightblue;";
			const string oneAndSixStyle = "background-color: deepSkyBlue;";
			const string twoAndFiveStyle = "background-color: blue;";
			const string threeAndFourStyle = "background-color: lightgreen;";
			const string fourAndThreeStyle = "background-color: green;";
			const string fiveAndTwoStyle = "background-color: yellow;";
			const string sixAndOneStyle = "background-color: red;";
			const string sevenAndZeroStyle = "background-color: black; color: white";

			var weekStyles = new Dictionary<DateOnly, string>();
			var week1StartDate = DateOnly.Parse("2013-09-15");
			
			foreach (var week in weeks)
			{
				var dayNumber = (week.WeekNumber - 1) * 7;
				var weekStartDay = week1StartDate.AddDays(dayNumber);
				var weekEndDay = weekStartDay.AddDays(6);

				for (var date = weekStartDay; date <= weekEndDay; date = date.AddDays(1))
				{
					weekStyles[date] = week.WorkedDays switch
					{
						0 => zeroAndSevenStyle,
						1 => oneAndSixStyle,
						2 => twoAndFiveStyle,
						3 => threeAndFourStyle,
						4 => fourAndThreeStyle,
						5 => fiveAndTwoStyle,
						6 => sixAndOneStyle,
						7 => sevenAndZeroStyle,
						_ => ""
					};
				}
			}

			var legend = new Dictionary<string, string>
			{
				["0-7"] = zeroAndSevenStyle,
				["1-6"] = oneAndSixStyle,
				["2-5"] = twoAndFiveStyle,
				["3-4"] = threeAndFourStyle,
				["4-3"] = fourAndThreeStyle,
				["5-2"] = fiveAndTwoStyle,
				["6-1"] = sixAndOneStyle,
				["7-0"] = sevenAndZeroStyle
			};

			var html = HtmlCalendarBuilder.BuildHtml(question, weekStyles, legend);
			writer.Write(html);
			writer.Close();
		}

		private void WeekTypesByWeekOfYear(string filePath)
		{
			const string question =
				"Mapped to weeks of the year, which week was most or least likely to be of each week type?";

			using var writer = OpenFile(filePath);
			writer.WriteLine(question);
			writer.WriteLine();
			
			var weekTypesByWeekNumber = new int[54, 7];
			var week1StartDate = DateOnly.Parse("2013-09-15");

			foreach (var week in weeks)
			{
				var dayNumber = (week.WeekNumber - 1) * 7;
				var weekStartDay = week1StartDate.AddDays(dayNumber);
				var iso8601WeekNumber = weekStartDay.GetIso8601WeekOfYear();
				var weekType = week.WorkedDays;

				weekTypesByWeekNumber[iso8601WeekNumber, weekType] += 1;
			}

			var weekBuffer = new int[7];
			for (int weekNumber = 0; weekNumber < 54; weekNumber++)
			{
				writer.WriteLine($"Week {weekNumber}:");

				for (int weekType = 0; weekType < 7; weekType++)
				{
					weekBuffer[weekType] = weekTypesByWeekNumber[weekNumber, weekType];
				}

				var weekTypesSortedByOccurence = weekBuffer.Select((c, i) => new
				{
					Occurences = c,
					WeekType = i
				})
				.OrderByDescending(a => a.Occurences)
				.Select(a => $"    {a.WeekType}-{7 - a.WeekType} weeks: {a.Occurences}");

				foreach (var weekType in weekTypesSortedByOccurence)
				{
					writer.WriteLine(weekType);
				}
				
				writer.WriteLine();
			}
			
			writer.Close();
		}

		private void SeriesCountsByType(string filePath)
		{
			const string question = "How many series of each type were there?";

			var seriesCountsByType = Enum.GetValues<MeijerDayType>()
				.Select(t => new
				{
					SeriesType = t,
					Count = series
						.Where(s => s.SeriesType == t)
						.MaxBy(s => s.SeriesNumberOfType)
						?.SeriesNumberOfType
						?? 0
				});

			var plot = new Plot();

			var bars = seriesCountsByType.Select(c => new Bar
			{
				Position = (int)c.SeriesType,
				Value = c.Count,
				FillColor = Color.FromARGB(0xFF0000FFu),
				Label = $"{c.Count}"
			});

			plot.Add.Bars(bars);

			var ticks = Enum.GetValues<MeijerDayType>().Select(t => new Tick((int)t, t.ToString())).ToArray();
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, size: 14f);

			plot.SavePng(filePath, 1024, 1024);
		}

		private void SeriesDistributionsByType(string filePath)
		{
			const string question = "What was the distribution of series by type, by length in days?";

			var seriesDistributionsByType = new Dictionary<MeijerDayType, SparseMaxArray<int>>();

			foreach (var meijerSeries in series)
			{
				var seriesType = meijerSeries.SeriesType;
				var seriesLength = meijerSeries.DaysInSeries;

				if (!seriesDistributionsByType.ContainsKey(seriesType))
				{
					seriesDistributionsByType[seriesType] = new SparseMaxArray<int>();
				}

				seriesDistributionsByType[seriesType][seriesLength] += 1;
			}

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();
			var ticks = new List<Tick>();
			var barPosition = 0;

			foreach (var kvp in seriesDistributionsByType)
			{
				for (var i = 1; i < kvp.Value.Count; i++)
				{
					var count = kvp.Value[i];

					var bar = new Bar
					{
						Position = barPosition,
						Value = count,
						FillColor = palette.GetColor((int)kvp.Key),
						Label = $"{count}"
					};

					var tick = new Tick(barPosition, $"{i} day");

					barPosition += 1;
					bars.Add(bar);
					ticks.Add(tick);
				}
			}

			plot.Add.Bars(bars);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperLeft;
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Working",
				FillColor = palette.GetColor((int)MeijerDayType.Working)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Working",
				FillColor = palette.GetColor((int)MeijerDayType.NonWorking)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.PaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.NonPaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Other",
				FillColor = palette.GetColor((int)MeijerDayType.Other)
			});
			
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);

			plot.SavePng(filePath, 1920, 1080);
		}

		private void TotalDaysSpentInEachSeriesByType(string filePath)
		{
			const string question = "How many days were spent total in each series by type of a given length?";

			var writer = OpenFile(filePath);
			writer.WriteLine(question);
			writer.WriteLine();

			var daysSpentInSeriesByLengthAndType = series
				.GroupBy(s => s.SeriesType)
				.Select(g => new
				{
					SeriesType = g.Key,
					DaysSpentInSeriesByLength = g
						.GroupBy(s => s.DaysInSeries)
						.Select(gc => new
						{
							SeriesLength = gc.Key,
							DaysSpent = gc.Sum(s => s.DaysInSeries)
						})
						.OrderBy(l => l.SeriesLength)
				});

			foreach (var byType in daysSpentInSeriesByLengthAndType)
			{
				writer.WriteLine($"{byType.SeriesType} series:");

				foreach (var byLength in byType.DaysSpentInSeriesByLength)
				{
					writer.WriteLine($"    {byLength.SeriesLength}-day series: {byLength.DaysSpent} days");
				}
			}
			
			writer.Close();
		}

		private void SeriesByTypeAndStartingWeekday(string filePath)
		{
			const string question = "What was the most likely day of the week a series would start?";

			var seriesByTypeAndStartDate = series.GroupBy(s => s.SeriesType)
				.Select(g => new
				{
					SeriesType = g.Key,
					CountsByFirstDayOfWeek = g.GroupBy(s => s.SeriesFirstDay.DayOfWeek)
						.Select(gw => new
						{
							DayOfWeek = gw.Key,
							Count = gw.Count()
						})
						.ToArray()
				});

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();
			var ticks = new List<Tick>();
			var barPosition = 0;

			foreach (var byType in seriesByTypeAndStartDate)
			{
				for (var dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					var count = byType.CountsByFirstDayOfWeek.SingleOrDefault(gw => gw.DayOfWeek == dayOfWeek)?.Count ?? 0;

					var bar = new Bar
					{
						Position = barPosition,
						Value = count,
						FillColor = palette.GetColor((int)byType.SeriesType),
						Label = $"{count}"
					};

					var tick = new Tick(barPosition, dayOfWeek.ToString());

					barPosition += 1;
					bars.Add(bar);
					ticks.Add(tick);
				}
			}

			plot.Add.Bars(bars);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperLeft;
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Working",
				FillColor = palette.GetColor((int)MeijerDayType.Working)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Working",
				FillColor = palette.GetColor((int)MeijerDayType.NonWorking)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.PaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.NonPaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Other",
				FillColor = palette.GetColor((int)MeijerDayType.Other)
			});

			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);

			plot.SavePng(filePath, 1920, 1080);
		}

		private void SeriesByTypeAndEndingWeekday(string filePath)
		{
			const string question = "What was the most likely day of the week a series would end?";

			var seriesByTypeAndStartDate = series.GroupBy(s => s.SeriesType)
				.Select(g => new
				{
					SeriesType = g.Key,
					CountsByLastDayOfWeek = g.GroupBy(s => s.SeriesLastDay.DayOfWeek)
						.Select(gw => new
						{
							DayOfWeek = gw.Key,
							Count = gw.Count()
						})
						.ToArray()
				});

			var plot = new Plot();
			var palette = new Category10();
			var bars = new List<Bar>();
			var ticks = new List<Tick>();
			var barPosition = 0;

			foreach (var byType in seriesByTypeAndStartDate)
			{
				for (var dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					var count = byType.CountsByLastDayOfWeek.SingleOrDefault(gw => gw.DayOfWeek == dayOfWeek)?.Count ?? 0;

					var bar = new Bar
					{
						Position = barPosition,
						Value = count,
						FillColor = palette.GetColor((int)byType.SeriesType),
						Label = $"{count}"
					};

					var tick = new Tick(barPosition, dayOfWeek.ToString());

					barPosition += 1;
					bars.Add(bar);
					ticks.Add(tick);
				}
			}

			plot.Add.Bars(bars);

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperLeft;
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Working",
				FillColor = palette.GetColor((int)MeijerDayType.Working)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Working",
				FillColor = palette.GetColor((int)MeijerDayType.NonWorking)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.PaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Non-Paid Time Off",
				FillColor = palette.GetColor((int)MeijerDayType.NonPaidTimeOff)
			});
			plot.Legend.ManualItems.Add(new LegendItem
			{
				LabelText = "Other",
				FillColor = palette.GetColor((int)MeijerDayType.Other)
			});

			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);

			plot.SavePng(filePath, 1920, 1080);
		}

		private void SeriesByTypeLengthAndStartingWeekday(string filePath)
		{
			const string question = "What was the most likely day of the week an N-day series would start?";

			var seriesByTypeLengthAndStartDay = new Dictionary<MeijerDayType, Dictionary<int, Dictionary<DayOfWeek, int>>>();

			foreach (var meijerSeries in series)
			{
				var seriesType = meijerSeries.SeriesType;

				if (!seriesByTypeLengthAndStartDay.ContainsKey(seriesType))
				{
					seriesByTypeLengthAndStartDay[seriesType] = new Dictionary<int, Dictionary<DayOfWeek, int>>();
				}

				var seriesByType = seriesByTypeLengthAndStartDay[seriesType];
				var seriesLength = meijerSeries.DaysInSeries;
				if (!seriesByType.ContainsKey(seriesLength))
				{
					seriesByType[seriesLength] = new Dictionary<DayOfWeek, int>();
				}

				var seriesByLength = seriesByType[seriesLength];
				var seriesFirstDayOfWeek = meijerSeries.SeriesFirstDay.DayOfWeek;
				if (!seriesByLength.TryAdd(seriesFirstDayOfWeek, 0))
				{
					seriesByLength[seriesFirstDayOfWeek] += 1;
				}
			}

			var plot = new Plot();
			var palettes = new Dictionary<MeijerDayType, IPalette>
			{
				[MeijerDayType.Working] = new ColorblindFriendly(),
				[MeijerDayType.NonWorking] = new Dark(),
				[MeijerDayType.PaidTimeOff] = new DarkPastel(),
				[MeijerDayType.NonPaidTimeOff] = new LightOcean(),
				[MeijerDayType.Other] = new Microcharts()
			};
			var bars = new List<List<Bar>>();
			var ticks = new List<Tick>();
			var barPosition = 0;

			foreach (var (seriesType, byLengths) in seriesByTypeLengthAndStartDay)
			{
				foreach (var (seriesLength, byFirstDaysOfWeek) in byLengths.OrderBy(l => l.Key))
				{
					var stackedBarBase = 0d;
					var currentBars = new List<Bar>();

					foreach (var (seriesFirstDayOfWeek, count) in byFirstDaysOfWeek.OrderBy(w => w.Key))
					{
						var bar = new Bar
						{
							Position = barPosition,
							Value = stackedBarBase + count,
							FillColor = palettes[seriesType].GetColor((int)seriesFirstDayOfWeek),
							ValueBase = stackedBarBase,
							Label = $"{count}",
							CenterLabel = true
						};
						
						currentBars.Add(bar);
						stackedBarBase += count;
					}
					
					bars.Add(currentBars);

					var tick = new Tick(barPosition, $"{seriesLength}d");
					ticks.Add(tick);

					barPosition += 1;
				}
			}

			plot.Add.Bars(bars.SelectMany(l => l));

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperRight;

			for (var seriesType = MeijerDayType.Working; seriesType <= MeijerDayType.Other; seriesType++)
			{
				for (var dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					plot.Legend.ManualItems.Add(new LegendItem
					{
						LabelText = $"{seriesType} {dayOfWeek}",
						FillColor = palettes[seriesType].GetColor((int)dayOfWeek)
					});
				}
			}

			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);
			
			plot.SavePng(filePath, 1920, 2160);
		}

		private void SeriesByTypeLengthAndEndingWeekday(string filePath)
		{
			const string question = "What was the most likely day of the week an N-day series would end?";

			var seriesByTypeLengthAndLastDay = new Dictionary<MeijerDayType, Dictionary<int, Dictionary<DayOfWeek, int>>>();

			foreach (var meijerSeries in series)
			{
				var seriesType = meijerSeries.SeriesType;

				if (!seriesByTypeLengthAndLastDay.ContainsKey(seriesType))
				{
					seriesByTypeLengthAndLastDay[seriesType] = new Dictionary<int, Dictionary<DayOfWeek, int>>();
				}

				var seriesByType = seriesByTypeLengthAndLastDay[seriesType];
				var seriesLength = meijerSeries.DaysInSeries;
				if (!seriesByType.ContainsKey(seriesLength))
				{
					seriesByType[seriesLength] = new Dictionary<DayOfWeek, int>();
				}

				var seriesByLength = seriesByType[seriesLength];
				var seriesLastDayOfWeek = meijerSeries.SeriesLastDay.DayOfWeek;
				if (!seriesByLength.TryAdd(seriesLastDayOfWeek, 0))
				{
					seriesByLength[seriesLastDayOfWeek] += 1;
				}
			}

			var plot = new Plot();
			var palettes = new Dictionary<MeijerDayType, IPalette>
			{
				[MeijerDayType.Working] = new ColorblindFriendly(),
				[MeijerDayType.NonWorking] = new Dark(),
				[MeijerDayType.PaidTimeOff] = new DarkPastel(),
				[MeijerDayType.NonPaidTimeOff] = new LightOcean(),
				[MeijerDayType.Other] = new Microcharts()
			};
			var bars = new List<List<Bar>>();
			var ticks = new List<Tick>();
			var barPosition = 0;

			foreach (var (seriesType, byLengths) in seriesByTypeLengthAndLastDay)
			{
				foreach (var (seriesLength, byLastDaysOfWeek) in byLengths.OrderBy(l => l.Key))
				{
					var stackedBarBase = 0d;
					var currentBars = new List<Bar>();

					foreach (var (seriesLastDayOfWeek, count) in byLastDaysOfWeek.OrderBy(w => w.Key))
					{
						var bar = new Bar
						{
							Position = barPosition,
							Value = stackedBarBase + count,
							FillColor = palettes[seriesType].GetColor((int)seriesLastDayOfWeek),
							ValueBase = stackedBarBase,
							Label = $"{count}",
							CenterLabel = true
						};

						currentBars.Add(bar);
						stackedBarBase += count;
					}

					bars.Add(currentBars);

					var tick = new Tick(barPosition, $"{seriesLength}d");
					ticks.Add(tick);

					barPosition += 1;
				}
			}

			plot.Add.Bars(bars.SelectMany(l => l));

			plot.Legend.IsVisible = true;
			plot.Legend.Alignment = Alignment.UpperRight;

			for (var seriesType = MeijerDayType.Working; seriesType <= MeijerDayType.Other; seriesType++)
			{
				for (var dayOfWeek = DayOfWeek.Sunday; dayOfWeek <= DayOfWeek.Saturday; dayOfWeek++)
				{
					plot.Legend.ManualItems.Add(new LegendItem
					{
						LabelText = $"{seriesType} {dayOfWeek}",
						FillColor = palettes[seriesType].GetColor((int)dayOfWeek)
					});
				}
			}

			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);

			plot.SavePng(filePath, 1920, 2160);
		}

		private void WorkingSeriesLengthsByDay(string filePath)
		{
			const string question = "Over all days, what was the length of each working series?";

			using var writer = OpenFile(filePath);

			const string oneDaySeriesStyle = "background-color: lightblue;";
			const string twoDaySeriesStyle = "background-color: deepSkyBlue;";
			const string threeDaySeriesStyle = "background-color: blue;";
			const string fourDaySeriesStyle = "background-color: lightgreen;";
			const string fiveDaySeriesStyle = "background-color: green;";
			const string sixDaySeriesStyle = "background-color: yellow;";
			const string sevenDaySeriesStyle = "background-color: red;";

			var seriesStyles = new Dictionary<DateOnly, string>();
			DateOnly.Parse("2013-09-15");

			foreach (var workingSeries in series.Where(s => s.SeriesType == MeijerDayType.Working))
			{
				for (var date = workingSeries.SeriesFirstDay;
				     date <= workingSeries.SeriesLastDay;
				     date = date.AddDays(1))
				{
					seriesStyles[date] = workingSeries.DaysInSeries switch
					{
						1 => oneDaySeriesStyle,
						2 => twoDaySeriesStyle,
						3 => threeDaySeriesStyle,
						4 => fourDaySeriesStyle,
						5 => fiveDaySeriesStyle,
						6 => sixDaySeriesStyle,
						7 => sevenDaySeriesStyle,
						_ => ""
					};
				}
			}

			var legend = new Dictionary<string, string>
			{
				["1-day working series"] = oneDaySeriesStyle,
				["2-day working series"] = twoDaySeriesStyle,
				["3-day working series"] = threeDaySeriesStyle,
				["4-day working series"] = fourDaySeriesStyle,
				["5-day working series"] = fiveDaySeriesStyle,
				["6-day working series"] = sixDaySeriesStyle,
				["7-day working series"] = sevenDaySeriesStyle
			};

			var html = HtmlCalendarBuilder.BuildHtml(question, seriesStyles, legend);
			writer.Write(html);
			writer.Close();
		}

		private void ExpectedDaysUntilNextDayOff(string filePath)
		{
			const string question = "If I've worked X days, how soon should I expect a day off?";

			var daysUntilNextDayOffByWorkedDays = new Dictionary<int, List<int>>();

			foreach (var workingSeries in series.Where(s => s.SeriesType == MeijerDayType.Working))
			{
				for (var workedDays = 1; workedDays <= workingSeries.DaysInSeries; workedDays++)
				{
					var daysLeftToWork = workingSeries.DaysInSeries - workedDays;

					if (!daysUntilNextDayOffByWorkedDays.TryAdd(workedDays, new List<int>()))
					{
						daysUntilNextDayOffByWorkedDays[workedDays].Add(daysLeftToWork);
					}
				}
			}

			var averageDaysLeftToWork = daysUntilNextDayOffByWorkedDays
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(l => (double)l).Average());
			var plot = new Plot();
			var bars = averageDaysLeftToWork.Select((kvp, i) => new Bar
			{
				Value = kvp.Value,
				Label = $"{kvp.Value:F2}",
				FillColor = Color.FromARGB(0xFF0000FFu),
				Position = i
			});
			var ticks = averageDaysLeftToWork.Keys
				.OrderBy(k => k)
				.Select(k => new Tick(k - 1d, $"{k} days"))
				.ToArray();

			plot.Add.Bars(bars);
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks);
			plot.Axes.Bottom.MajorTickStyle = new TickMarkStyle
			{
				Length = 0f
			};
			plot.HideGrid();
			plot.Axes.Margins(bottom: 0d);
			plot.Title(question, 18f);
			
			plot.SavePng(filePath, 1920, 1080);
		}

		private void LongestStretchOfNDaySeriesOfType(string filePath)
		{
			const string question = "What was the longest stretch of series of at least N days?";

			var writer = OpenFile(filePath);
			writer.WriteLine(question);
			writer.WriteLine();
			
			var seriesStretchesByType = Enum.GetValues<MeijerDayType>()
				.Select(t => new
				{
					SeriesType = t,
					Stretches = GetLongestStretchOfSeries(series.Where(s => s.SeriesType == t))
				});

			foreach (var stretchesByType in seriesStretchesByType)
			{
				writer.WriteLine($"{stretchesByType.SeriesType}:");

				foreach (var kvp in stretchesByType.Stretches.OrderBy(kvp => kvp.Key))
				{
					writer.WriteLine($"{kvp.Key}-day series: Stretch of {kvp.Value} in a row ({kvp.Key * kvp.Value} days)");
				}

				writer.WriteLine();
			}

			writer.Close();

			return;

			Dictionary<int, int> GetLongestStretchOfSeries(IEnumerable<MeijerSeries> meijerSeries)
			{
				var currentStretchLength = 0;
				var currentStretchCount = 0;
				var result = new Dictionary<int, int>();
				
				foreach (var singleSeries in meijerSeries)
				{
					if (singleSeries.DaysInSeries == currentStretchLength)
					{
						currentStretchCount += 1;
					}
					else
					{
						if (!result.TryAdd(currentStretchLength, 1))
						{
							if (result[currentStretchLength] < currentStretchCount)
							{
								result[currentStretchLength] = currentStretchCount;
							}
						}
						
						currentStretchLength = singleSeries.DaysInSeries;
						currentStretchCount = 1;
					}
				}

				return result;
			}
		}

		private void LongestGapBetweenNDaySeriesOfType(string filePath)
		{
			const string question = "What was the longest gap between series of at least N days?";

			var writer = OpenFile(filePath);
			writer.WriteLine(question);
			writer.WriteLine();

			var seriesGapsByType = Enum.GetValues<MeijerDayType>()
				.Select(t => new
				{
					SeriesType = t,
					Gaps = GetLongestGapBetweenSeries(series.Where(s => s.SeriesType == t).ToArray())
				});

			foreach (var gapsByType in seriesGapsByType)
			{
				writer.WriteLine($"{gapsByType.SeriesType}:");

				foreach (var kvp in gapsByType.Gaps.OrderBy(kvp => kvp.Key))
				{
					writer.WriteLine($"{kvp.Key}-day series: Gap of {kvp.Value} series");
				}

				writer.WriteLine();
			}

			writer.Close();

			return;

			Dictionary<int, int> GetLongestGapBetweenSeries(IList<MeijerSeries> meijerSeries)
			{
				if (meijerSeries.Count == 0) { return new(); }
				
				var result = new Dictionary<int, int>();
				var longestSeries = meijerSeries.MaxBy(s => s.DaysInSeries)!.DaysInSeries;
				var currentSeriesGaps = new int[longestSeries];
				for (var i = 1; i <= longestSeries; i++) { result.Add(i, 0); }

				foreach (var singleSeries in meijerSeries)
				{
					var index = singleSeries.DaysInSeries - 1;

					if (currentSeriesGaps[index] > result[singleSeries.DaysInSeries])
					{
						result[singleSeries.DaysInSeries] = currentSeriesGaps[index];
					}

					for (var i = 0; i < currentSeriesGaps.Length; i++)
					{
						if (i == index) { currentSeriesGaps[index] = 0; }
						else { currentSeriesGaps[i] += 1; }
					}
				}

				return result;
			}
		}

		private void WorkingVsNonWorkingSeries(string filePath)
		{
			const string question = "How many non-working series were there?";

			var writer = OpenFile(filePath);
			writer.WriteLine(question);
			writer.WriteLine();
			writer.WriteLine($"Working series: {series.Count(t => t.SeriesType is MeijerDayType.Working or MeijerDayType.Other)} series");
			writer.WriteLine($"Non-working series: {series.Count(t => t.SeriesType is not MeijerDayType.Working and not MeijerDayType.Other)} series");
			
			writer.Close();
		}

		private void LongestStretchOfNonWorkingSeriesOfAtLeastNDays(string filePath)
		{
			const string question = "What was the longest stretch of non-working series of at least N days?";

			var nonWorkingSeries = series.Where(s =>
				s.SeriesType != MeijerDayType.Working && s.SeriesType != MeijerDayType.Other)
				.ToArray();
			var longestNonWorkingSeriesLength = nonWorkingSeries.MaxBy(s => s.DaysInSeries)!.DaysInSeries;
			var longestStretches = new int[longestNonWorkingSeriesLength];

			for (var i = 0; i < longestNonWorkingSeriesLength; i++)
			{
				var currentStretch = 0;
				var longestStretch = 0;
				
				foreach (var singleSeries in nonWorkingSeries)
				{
					if (singleSeries.DaysInSeries >= i + 1)
					{
						currentStretch += 1;
					}
					else
					{
						if (currentStretch > longestStretch)
						{
							longestStretch = currentStretch;
						}

						currentStretch = 0;
					}
				}

				if (longestStretch == 0) { longestStretch = currentStretch; }
				longestStretches[i] = longestStretch;
			}

			var plot = new Plot();

			var bars = longestStretches.Select((s, i) => new Bar
			{
				Position = i + 1,
				Value = s,
				FillColor = Color.FromARGB(0xFF0000FFu),
				Label = $"{s}"
			});
			var ticks = longestStretches.Select((_, i) => new Tick(i + 1, $">= {i + 1} day"));

			plot.Add.Bars(bars);
			plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
			plot.Axes.Bottom.MajorTickStyle.Length = 0f;
			plot.HideGrid();

			plot.Title(question);
			plot.SavePng(filePath, 1920, 1080);
		}
	}
}
