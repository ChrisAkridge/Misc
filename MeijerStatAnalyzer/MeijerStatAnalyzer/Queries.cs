using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	internal static class Queries
	{
		// Query: a static method accepting a StatsByDay instance that runs
		// some LINQ. Put your calls in RunQuery().

		public static void RunQuery(StatsByDay stats)
		{
			//const int overallDays = 1706;
			//Console.WriteLine($"Working days: {stats.TotalWorkingDays}");
			//Console.WriteLine($"Working Days/Overall Days: {GetPercent(stats.TotalWorkingDays, overallDays)}");

			Console.WriteLine(stats.Days.Count(d => d.Type == DayType.Working));
			Console.WriteLine(stats.Days.Count(d => d.Shift != null));

			var days = stats.Days.Where(d => d.Shift != null).Except(stats.Days.Where(d => d.Type == DayType.Working));

			foreach (var day in days)
			{
				Console.WriteLine(day.ToString());
			}
		}

		private static void PrintShiftCount(StatsByDay stats)
		{
			IEnumerable<Shift> shifts = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift);
			int openCount = shifts.Where(IsOpeningShift).Count();
			int midCount = shifts.Where(IsMidshift).Count();
			int closeCount = shifts.Count(IsClosingShift);

			Console.WriteLine($"Of {shifts.Count()} shifts, {openCount} ({GetPercent(openCount, shifts.Count())}) were opening shifts.");
			Console.WriteLine($"{midCount} ({GetPercent(midCount, shifts.Count())}) were midshifts.");
			Console.WriteLine($"{closeCount} ({GetPercent(closeCount, shifts.Count())}) were closing shifts.");
		}

		private static void PrintWeekdayInfo(StatsByDay stats) => PrintDayInfoByProperty(stats, d => d.Date.DayOfWeek);
		private static void PrintMonthInfo(StatsByDay stats) => PrintDayInfoByProperty(stats, d => d.Date.Month);
		private static void PrintDayOfMonthInfo(StatsByDay stats) => PrintDayInfoByProperty(stats, d => d.Date.Day);
		private static void PrintYearInfo(StatsByDay stats) => PrintDayInfoByProperty(stats, d => d.Date.Year);

		private static void PrintDayInfoByProperty<T>(StatsByDay stats, Func<Day, T> selector)
		{
			Dictionary<T, IGrouping<T, Day>> groupedDays =
				stats.Days.GroupBy(selector).OrderBy(g => g.Key).ToDictionary(g => g.Key);

			foreach (KeyValuePair<T, IGrouping<T, Day>> kvp in groupedDays)
			{
				PrintSingleDayInfo(kvp.Key, kvp.Value);
			}
		}

		private static void PrintSingleDayInfo<T>(T key, IEnumerable<Day> days)
		{
			int totalCount = days.Count();
			int workingCount = days.Count(d => d.Shift != null);

			Dictionary<ShiftType, IGrouping<ShiftType, Shift>> shiftsByType = 
				days.Where(d => d.Shift != null).Select(d => d.Shift).GroupBy(GetShiftType).ToDictionary(g => g.Key);
			int open = (shiftsByType.ContainsKey(ShiftType.Opening)) ? shiftsByType[ShiftType.Opening].Count() : 0;
			int mid = (shiftsByType.ContainsKey(ShiftType.Midshift)) ? shiftsByType[ShiftType.Midshift].Count() : 0;
			int close = (shiftsByType.ContainsKey(ShiftType.Closing)) ? shiftsByType[ShiftType.Closing].Count() : 0;

			Console.WriteLine($"{key.ToString()}: Worked {workingCount} of {totalCount} ({GetPercent(workingCount, totalCount)})");
			Console.WriteLine($"\t{open} opens ({GetPercent(open, workingCount)}), {mid} midshifts ({GetPercent(mid, workingCount)}), {close} closes ({GetPercent(close, workingCount)})");
		}

		private static void PrintAverageShiftStarts(StatsByDay stats)
		{
			DateTime cutoff = new DateTime(2013, 9, 17);
			IEnumerable<Day> subset = stats.Days.Where(d => d.Date >= cutoff);

			int averageOpenSeconds = 0;
			int averageMidSeconds = 0;
			int averageCloseSeconds = 0;

			int opens = 0;
			int mids = 0;
			int closes = 0;

			foreach (Day day in subset)
			{
				if (day.Shift == null) { continue; }
				Shift shift = day.Shift;

				if (IsOpeningShift(shift))
				{
					averageOpenSeconds += shift.StartTime.Seconds;
					opens++;
				}
				if (IsMidshift(shift))
				{
					averageMidSeconds += shift.StartTime.Seconds;
					mids++;
				}
				if (IsClosingShift(shift))
				{
					averageCloseSeconds += shift.StartTime.Seconds;
					closes++;
				}
			}

			var averageOpen = new SecondsAfterMidnight(averageOpenSeconds / opens);
			var averageMid = new SecondsAfterMidnight(averageMidSeconds / mids);
			var averageClose = new SecondsAfterMidnight(averageCloseSeconds / closes);

			Console.WriteLine($"Average of {opens} opens: {averageOpen.ToString()}");
			Console.WriteLine($"Average of {mids} midshifts: {averageMid.ToString()}");
			Console.WriteLine($"Average of {closes} closes: {averageClose.ToString()}");
		}

		private static void PrintAverageShiftEnds(StatsByDay stats)
		{
			var cutoff = new DateTime(2013, 9, 17);
			IEnumerable<Day> subset = stats.Days.Where(d => d.Date >= cutoff);

			int averageOpenSeconds = 0;
			int averageMidSeconds = 0;
			int averageCloseSeconds = 0;

			int opens = 0;
			int mids = 0;
			int closes = 0;

			foreach (Day day in subset)
			{
				if (day.Shift == null) { continue; }
				Shift shift = day.Shift;

				if (IsOpeningShift(shift))
				{
					averageOpenSeconds += shift.EndTime.Seconds;
					opens++;
				}
				if (IsMidshift(shift))
				{
					averageMidSeconds += shift.EndTime.Seconds;
					mids++;
				}
				if (IsClosingShift(shift))
				{
					averageCloseSeconds += shift.EndTime.Seconds;
					closes++;
				}
			}

			var averageOpen = new SecondsAfterMidnight(averageOpenSeconds / opens);
			var averageMid = new SecondsAfterMidnight(averageMidSeconds / mids);
			var averageClose = new SecondsAfterMidnight(averageCloseSeconds / closes);

			Console.WriteLine($"Average of {opens} opens: {averageOpen.ToString()}");
			Console.WriteLine($"Average of {mids} midshifts: {averageMid.ToString()}");
			Console.WriteLine($"Average of {closes} closes: {averageClose.ToString()}");
		}

		private static string ProduceHourByHourInfo(StatsByDay stats)
		{
			const char off = 'O';
			const char pregame = 'P';
			const char wait = '.';
			const char work = 'W';

			int totalHours = (int)(stats.Days.Last().Date - stats.Days.First().Date).TotalHours;
			var result = new StringBuilder();

			for (int overallHour = 0; overallHour <= totalHours; overallHour += 24)
			{
				int dayNumber = overallHour / 24;

				Day day = stats.Days[dayNumber];
				for (int i = 0; i < 24; i++)
				{
					switch (GetHourType(day, i))
					{
						case HourType.Off:
							result.Append(off);
							break;
						case HourType.Pregame:
							result.Append(pregame);
							break;
						case HourType.Wait:
							result.Append(wait);
							break;
						case HourType.Work:
							result.Append(work);
							break;
						default:
							break;
					}
				}
			}

			return result.ToString();
		}

		private static Tuple<double, double> GetAverageWeek(StatsByDay stats)
		{
			int onSum = stats.Weeks.Sum(w => w.WorkingDays);
			int offSum = stats.Weeks.Sum(w => w.DaysOff);

			double workingAverage = (double)onSum / stats.Weeks.Count;
			double offAverage = (double)offSum / stats.Weeks.Count;
			return new Tuple<double, double>(workingAverage, offAverage);
		}

		private static string GetAverageStartTime(StatsByDay stats)
		{
			IEnumerable<SecondsAfterMidnight> startTimes = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift.StartTime);
			return new SecondsAfterMidnight((startTimes.Sum(s => s.Seconds) / startTimes.Count())).ToTimeString();
		}

		private static string GetAverageEndTime(StatsByDay stats)
		{
			IEnumerable<SecondsAfterMidnight> endTimes = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift.EndTime);
			return new SecondsAfterMidnight((endTimes.Sum(e => e.Seconds) / endTimes.Count())).ToTimeString();
		}

		// Shift info
		private static ShiftType GetShiftType(Shift shift)
		{
			if (IsOpeningShift(shift)) { return ShiftType.Opening; }
			else if (IsMidshift(shift)) { return ShiftType.Midshift; }
			return ShiftType.Closing;
		}

		private static bool IsOpeningShift(Shift shift)
		{
			// Opening shift: starts between 5am and 8am
			// or, if it's on 2014-01-17, it's not an opening shift
			// or, after 2017-05-09, starts between 5am and 10am

			var fiveAM = new SecondsAfterMidnight(5, 0, 0);
			var eightAM = new SecondsAfterMidnight(8, 0, 0);
			var tenAM = new SecondsAfterMidnight(10, 0, 0);
			var threeThirtyPM = new SecondsAfterMidnight(15, 30, 0);

			if (shift.Date == new DateTime(2014, 01, 17)) { return false; }

			if (shift.Date < new DateTime(2017, 5, 9))
			{
				return (shift.StartTime >= fiveAM && shift.StartTime <= eightAM);
			}
			else
			{
				return (shift.StartTime >= fiveAM && shift.StartTime <= tenAM);
			}
		}

		private static bool IsMidshift(Shift shift)
		{
			// Midshift: starts after 8am ends on or before 8pm
			// or, if it's on 2014-01-17, it's a midshift
			// or, after 2017-05-09, starts after 10am

			var eightAM = new SecondsAfterMidnight(8, 0, 0);
			var tenAM = new SecondsAfterMidnight(10, 0, 0);
			var eightPM = new SecondsAfterMidnight(20, 0, 0);

			if (shift.Date == new DateTime(2014, 01, 17)) { return true; }

			if (shift.Date < new DateTime(2017, 5, 9))
			{
				return (shift.StartTime > eightAM) && (shift.EndTime <= eightPM);
			}
			else
			{
				return (shift.StartTime > tenAM) && (shift.EndTime <= eightPM); 
			}
		}

		private static bool IsClosingShift(Shift shift)
		{
			// Closing shift: ends between 8pm and 11:59:59pm

			var eightPM = new SecondsAfterMidnight(20, 0, 0);
			var secondBeforeMidnight = new SecondsAfterMidnight(86399);

			return (shift.EndTime > eightPM && shift.EndTime <= secondBeforeMidnight);
		}

		private static bool ShiftRequiredEarlyRise(Shift shift)
		{
			var waitStart = new SecondsAfterMidnight(shift.StartTime.Seconds - (int)(shift.WaitHours * 3600d));
			return (waitStart < new SecondsAfterMidnight(10, 0, 0));
		}

		private static SecondsAfterMidnight EstimateAlarm(Shift shift)
		{
			var waitStart = new SecondsAfterMidnight(shift.StartTime.Seconds - (int)(shift.WaitHours * 3600d));
			return new SecondsAfterMidnight(waitStart.Seconds - 3600);
		}

		private static IEnumerable<T> SortSeriesBy<T>(StatsByDay stats, Func<Series, T> selector) where T : IComparable<T>
			=> stats.Series.OrderBy(selector).Select(selector);

		// Hour info
		private static HourType GetHourType(Day day, int hour)
		{
			if (day.Shift == null) { return HourType.Off; }
			else
			{
				var hourStart = new SecondsAfterMidnight(hour, 0, 0);
				SecondsAfterMidnight pregameStart = !ShiftRequiredEarlyRise(day.Shift) 
					? new SecondsAfterMidnight(day.Shift.StartTime.Seconds - 4500)
					: EstimateAlarm(day.Shift);
				var waitStart = new SecondsAfterMidnight(day.Shift.StartTime.Seconds - (int)(day.Shift.WaitHours * 3600d));

				if (hourStart < pregameStart) { return HourType.Off; }
				else if (hourStart >= pregameStart && hourStart < waitStart) { return HourType.Pregame; }
				else if (hourStart >= waitStart && hourStart < day.Shift.StartTime) { return HourType.Wait; }
				else if (hourStart >= day.Shift.StartTime && hourStart < day.Shift.EndTime) { return HourType.Work; }
				else { return HourType.Off; }
			}
		}

		// Formatters
		private static string GetPercent(double a, double b) => $"{(a / b) * 100:F2}%";
	}

	internal enum ShiftType
	{
		Opening,
		Midshift,
		Closing
	}

	internal enum HourType
	{
		Off,
		Pregame,
		Wait,
		Work
	}
}
