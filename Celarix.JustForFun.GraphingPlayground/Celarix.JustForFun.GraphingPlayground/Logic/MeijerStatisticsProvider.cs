using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models;
using ClosedXML.Excel;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal sealed class MeijerStatisticsProvider : IDisposable
	{
		private const string StatisticsExcelPath =
			@"F:\Documents\Documents\Documents\Just for Fun\Meijer - Employment Statistics 2.0.xlsx";

		private readonly XLWorkbook statisticsSheet;

		public MeijerStatisticsProvider()
		{
			statisticsSheet = new XLWorkbook(StatisticsExcelPath);
		}

		public IReadOnlyList<MeijerDay> GetDays()
		{
			if (!statisticsSheet.Worksheets.TryGetWorksheet("Stats", out var statsWorksheet))
			{
				throw new InvalidOperationException($"The workbook at \"{StatisticsExcelPath}\" is bad!");
			}

			const string weekNumberColumn = "A";
			const string dateColumn = "C";
			const string scheduleColumn = "D";
			const string overallDayNumberColumn = "E";
			const string dayNumberOfTypeColumn = "F";
			const string hoursColumn = "G";
			const string waitColumn = "I";
			const string paidHoursColumn = "J";
			const string unpaidHoursColumn = "K";
			const string payColumn = "L";
			
			const int lastRowNumber = 1714;

			var seenWorkingDays = 0;
			var seenNonWorkingDays = 0;
			var seenPaidTimeOffDays = 0;
			var seenNonPaidTimeOffDays = 0;
			var seenOtherDays = 0;

			var days = new List<MeijerDay>();

			for (var rowNumber = 8; rowNumber <= lastRowNumber; rowNumber++)
			{
				var weekNumber = GetWeekNumberFromRowNumber(rowNumber);

				var dateCell = statsWorksheet.Cell(dateColumn + rowNumber).GetDateTime();
				var date = new DateOnly(dateCell.Year, dateCell.Month, dateCell.Day);

				var scheduleCellValue = statsWorksheet.Cell(scheduleColumn + rowNumber).Value;
				var schedulePresent = !scheduleCellValue.IsBlank && scheduleCellValue.GetText().Contains('-');
				var scheduleText = schedulePresent ? scheduleCellValue.GetText() : null;
				var scheduleParts = scheduleText?
					.Split('-')
					.Select(p => p.TrimStart('*').TrimEnd('*'))
					.ToArray();
				TimeOnly? shiftStart = null;
				TimeOnly? shiftEnd = null;

				if (schedulePresent)
				{
					if (!scheduleParts![0].EndsWith("am", StringComparison.InvariantCultureIgnoreCase)
					    && !scheduleParts[0].EndsWith("pm", StringComparison.InvariantCultureIgnoreCase))
					{
						var meridiem = scheduleParts[1].Substring(scheduleParts[1].Length - 2, 2);
						scheduleParts[0] += meridiem;
					}
					
					shiftStart = TimeOnly.Parse(scheduleParts[0], CultureInfo.InvariantCulture);
					shiftEnd = TimeOnly.Parse(scheduleParts[1], CultureInfo.InvariantCulture);
				}

				var overallDayNumberCell = statsWorksheet.Cell(overallDayNumberColumn + rowNumber).GetDouble();
				var overallDayNumber = (int)overallDayNumberCell;

				var dayNumberOfTypeCell = statsWorksheet.Cell(dayNumberOfTypeColumn + rowNumber).GetText();
				var dayType = new string(dayNumberOfTypeCell.TakeWhile(char.IsLetter).ToArray()).ToLowerInvariant() switch
				{
					"w" => MeijerDayType.Working,
					"n" => MeijerDayType.NonWorking,
					"pv" => MeijerDayType.PaidTimeOff,
					"nv" => MeijerDayType.NonPaidTimeOff,
					"o" => MeijerDayType.Other,
					_ => throw new InvalidOperationException($"Unknown day type \"{dayNumberOfTypeCell}\"")
				};

				var hoursCellValue = statsWorksheet.Cell(hoursColumn + rowNumber).Value;
				var hoursPresent = !hoursCellValue.IsBlank;
				var hours = hoursPresent
					? (decimal?)hoursCellValue.GetNumber()
					: null;

				var waitCellValue = statsWorksheet.Cell(waitColumn + rowNumber).Value;
				var waitPresent = !waitCellValue.IsBlank;
				var wait = waitPresent
					? (decimal?)waitCellValue.GetNumber()
					: null;
				
				var paidHoursCellValue = statsWorksheet.Cell(paidHoursColumn + rowNumber).Value;
				var paidHoursPresent = !paidHoursCellValue.IsBlank;
				var paidHours = paidHoursPresent
					? (decimal?)paidHoursCellValue.GetNumber()
					: null;
				
				var unpaidHoursCellValue = statsWorksheet.Cell(unpaidHoursColumn + rowNumber).Value;
				var unpaidHoursPresent = !unpaidHoursCellValue.IsBlank;
				var unpaidHours = unpaidHoursPresent
					? (decimal?)unpaidHoursCellValue.GetNumber()
					: null;
				
				var payCellValue = statsWorksheet.Cell(payColumn + rowNumber).Value;
				var payPresent = !payCellValue.IsBlank;
				var pay = payPresent
					? (decimal?)payCellValue.GetNumber()
					: null;

				if (hours == null && dayType is MeijerDayType.Working or MeijerDayType.Other)
				{
					dayType = MeijerDayType.NonWorking;
				}

				switch (dayType)
				{
					case MeijerDayType.Working:
						seenWorkingDays += 1;
						break;
					case MeijerDayType.NonWorking:
						seenNonWorkingDays += 1;
						break;
					case MeijerDayType.PaidTimeOff:
						seenPaidTimeOffDays += 1;
						break;
					case MeijerDayType.NonPaidTimeOff:
						seenNonPaidTimeOffDays += 1;
						break;
					case MeijerDayType.Other:
						seenOtherDays += 1;
						break;
					default:
						throw new InvalidOperationException($"Unknown day type \"{dayType}\"");
				}

				days.Add(new MeijerDay
				{
					WeekNumber = weekNumber,
					Date = date,
					ShiftStart = shiftStart,
					ShiftEnd = shiftEnd,
					OverallDayNumber = overallDayNumber,
					DayType = dayType,
					DayNumberOfType = dayType switch
					{
						MeijerDayType.Working => seenWorkingDays,
						MeijerDayType.NonWorking => seenNonWorkingDays,
						MeijerDayType.PaidTimeOff => seenPaidTimeOffDays,
						MeijerDayType.NonPaidTimeOff => seenNonPaidTimeOffDays,
						MeijerDayType.Other => seenOtherDays,
						_ => throw new InvalidOperationException($"Unknown day type \"{dayType}\"")
					},
					HoursWorked = hours,
					HoursWaited = wait,
					PaidHours = paidHours,
					UnpaidHours = unpaidHours,
					Pay = pay
				});
			}

			return days;
		}

		public IReadOnlyList<MeijerWeek> GetWeeks(IReadOnlyList<MeijerDay> days)
		{
			var weeks = new List<MeijerWeek>();
			var currentWeekNumber = 1;
			var currentWeekWorkingDays = 0;
			var currentWeekOffDays = 0;

			foreach (var day in days)
			{
				if (day.Weekday == DayOfWeek.Sunday)
				{
					weeks.Add(new MeijerWeek
					{
						WeekNumber = currentWeekNumber,
						WorkedDays = currentWeekWorkingDays,
						OffDays = currentWeekOffDays
					});

					currentWeekNumber += 1;
					currentWeekWorkingDays = 0;
					currentWeekOffDays = 0;
				}
				
				if (day.DayType is MeijerDayType.Working or MeijerDayType.Other)
				{
					currentWeekWorkingDays += 1;
				}
				else
				{
					currentWeekOffDays += 1;
				}
			}
			
			weeks.Add(new MeijerWeek
			{
				WeekNumber = currentWeekNumber,
				WorkedDays = currentWeekWorkingDays,
				OffDays = currentWeekOffDays
			});

			return weeks;
		}
		
		public IReadOnlyList<MeijerSeries> GetSeries(IReadOnlyList<MeijerDay> days)
		{
			var series = new List<MeijerSeries>();
			var daysInSeries = new List<MeijerDay>();

			var seenWorkingSeries = 0;
			var seenNonWorkingSeries = 0;
			var seenPaidTimeOffSeries = 0;
			var seenNonPaidTimeOffSeries = 0;
			var seenOtherSeries = 0;
			
			var currentSeriesNumber = 1;
			var currentSeriesType = days[0].DayType;
			var currentSeriesDays = 1;
			var currentSeriesHours = days[0].HoursWorked ?? 0;
			var currentSeriesFirstDay = days[0].Date;
			var currentSeriesLastDay = days[0].Date;
			daysInSeries.Add(days[0]);

			switch (days[0].DayType)
			{
				case MeijerDayType.Working:
					seenWorkingSeries += 1;

					break;
				case MeijerDayType.NonWorking:
					seenNonWorkingSeries += 1;

					break;
				case MeijerDayType.PaidTimeOff:
					seenPaidTimeOffSeries += 1;

					break;
				case MeijerDayType.NonPaidTimeOff:
					seenNonPaidTimeOffSeries += 1;

					break;
				case MeijerDayType.Other:
					seenOtherSeries += 1;

					break;
				default:
					throw new InvalidOperationException($"Unknown day type \"{days[0].DayType}\"");
			}

			for (var i = 1; i < days.Count; i++)
			{
				var day = days[i];

				if (day.DayType == currentSeriesType)
				{
					currentSeriesDays += 1;
					currentSeriesHours += day.HoursWorked ?? 0;
					currentSeriesLastDay = day.Date;
					daysInSeries.Add(day);
				}
				else
				{
					series.Add(new MeijerSeries
					{
						OverallSeriesNumber = currentSeriesNumber,
						SeriesType = currentSeriesType,
						SeriesNumberOfType = currentSeriesType switch
						{
							MeijerDayType.Working => seenWorkingSeries,
							MeijerDayType.NonWorking => seenNonWorkingSeries,
							MeijerDayType.PaidTimeOff => seenPaidTimeOffSeries,
							MeijerDayType.NonPaidTimeOff => seenNonPaidTimeOffSeries,
							MeijerDayType.Other => seenOtherSeries,
							_ => throw new InvalidOperationException($"Unknown day type \"{currentSeriesType}\"")
						},
						DaysInSeries = currentSeriesDays,
						HoursInSeries = currentSeriesHours,
						SeriesFirstDay = currentSeriesFirstDay,
						SeriesLastDay = currentSeriesLastDay,
						SeriesMidway = GetSeriesMidway(daysInSeries, currentSeriesType)
					});

					currentSeriesNumber += 1;
					currentSeriesType = day.DayType;
					currentSeriesFirstDay = day.Date;
					currentSeriesLastDay = day.Date;

					switch (day.DayType)
					{
						case MeijerDayType.Working:
							seenWorkingSeries += 1;
							break;
						case MeijerDayType.NonWorking:
							seenNonWorkingSeries += 1;
							break;
						case MeijerDayType.PaidTimeOff:
							seenPaidTimeOffSeries += 1;
							break;
						case MeijerDayType.NonPaidTimeOff:
							seenNonPaidTimeOffSeries += 1;
							break;
						case MeijerDayType.Other:
							seenOtherSeries += 1;
							break;
						default:
							throw new InvalidOperationException($"Unknown day type \"{day.DayType}\"");
					}
					
					currentSeriesDays = 1;
					currentSeriesHours = day.HoursWorked ?? 0;
					daysInSeries.Clear();
					daysInSeries.Add(day);
				}
			}

			series.Add(new MeijerSeries
			{
				OverallSeriesNumber = currentSeriesNumber,
				SeriesType = currentSeriesType,
				SeriesNumberOfType = currentSeriesType switch
				{
					MeijerDayType.Working => seenWorkingSeries,
					MeijerDayType.NonWorking => seenNonWorkingSeries,
					MeijerDayType.PaidTimeOff => seenPaidTimeOffSeries,
					MeijerDayType.NonPaidTimeOff => seenNonPaidTimeOffSeries,
					MeijerDayType.Other => seenOtherSeries,
					_ => throw new InvalidOperationException($"Unknown day type \"{currentSeriesType}\"")
				},
				DaysInSeries = currentSeriesDays,
				HoursInSeries = currentSeriesHours,
				SeriesMidway = GetSeriesMidway(daysInSeries, currentSeriesType)
			});

			return series;
		}

		/// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
		public void Dispose() { statisticsSheet.Dispose(); }

		private int GetWeekNumberFromRowNumber(int rowNumber)
		{
			// There are 7 non-data rows at the top of the file, so subtract 7 from the row number
			var dayNumber = rowNumber - 7;

			return (dayNumber / 7) + 1;
		}

		private DateTimeOffset GetSeriesMidway(IReadOnlyList<MeijerDay> daysInSeries, MeijerDayType seriesType)
		{
			if (seriesType is MeijerDayType.Working or MeijerDayType.Other)
			{
				var hoursInDays = daysInSeries.Select(d => d.ShiftEnd! - d.ShiftStart!);
				var hoursInSeries = hoursInDays.Sum(h => h!.Value.TotalHours);
				var midway = (decimal)(hoursInSeries / 2d);

				foreach (var day in daysInSeries)
				{
					var hoursInDay = day.ShiftEnd! - day.ShiftStart!;

					if (midway <= (decimal)hoursInDay!.Value.TotalHours)
					{
						var shiftStartDateTime =
							new DateTimeOffset(day.Date, day.ShiftStart!.Value, day.Date.GetTimezoneOffsetByDate());

						return shiftStartDateTime.AddHours((double)midway);
					}

					midway -= (decimal)hoursInDay.Value.TotalHours;
				}

				throw new InvalidOperationException("Fell out of a series without finding its midway.");
			}
			else
			{
				var seriesFirstDay = daysInSeries.First();
				var seriesLastDay = daysInSeries.Last();

				var seriesStart = new DateTimeOffset(seriesFirstDay.Date, TimeOnly.MinValue,
					seriesFirstDay.Date.GetTimezoneOffsetByDate());
				var seriesEnd = new DateTimeOffset(seriesLastDay.Date.AddDays(1), TimeOnly.MinValue,
					seriesLastDay.Date.AddDays(1).GetTimezoneOffsetByDate());

				var seriesDuration = seriesEnd - seriesStart;
				var midway = seriesDuration.TotalHours / 2d;

				return seriesStart + TimeSpan.FromHours(midway);
			}
		}
	}
}
