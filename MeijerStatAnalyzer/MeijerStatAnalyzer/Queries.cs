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
            DateTime cutoff = new DateTime(2016, 6, 1);
            var subset = stats.Days.Where(d => d.Date >= cutoff);

            int averageOpenSeconds = 0;
            int averageMidSeconds = 0;
            int averageCloseSeconds = 0;

            int opens = 0;
            int mids = 0;
            int closes = 0;

            foreach (var day in subset)
            {
                if (day.Shift == null) { continue; }
                Shift shift = day.Shift;
                switch (shift.ShiftTime)
                {
                    case ShiftTime.Open:
                        averageOpenSeconds += shift.StartTime.Seconds;
                        opens++;
                        break;
                    case ShiftTime.Midshift:
                        averageMidSeconds += shift.StartTime.Seconds;
                        mids++;
                        break;
                    case ShiftTime.Close:
                        averageCloseSeconds += shift.StartTime.Seconds;
                        closes++;
                        break;
                    case ShiftTime.ThirdShift:
                        break;
                    default:
                        break;
                }
            }

            var averageOpen = new SecondsAfterMidnight(averageOpenSeconds / opens);
            var averageMid = new SecondsAfterMidnight(averageMidSeconds / mids);
            var averageClose = new SecondsAfterMidnight(averageCloseSeconds / closes);

            Console.WriteLine($"Average of {opens} opens: {averageOpen.ToString()}");
            Console.WriteLine($"Average of {mids} midshifts: {averageMid.ToString()}");
            Console.WriteLine($"Average of {closes} closes: {averageClose.ToString()}");
        }

		private static void PrintShiftCount(StatsByDay stats)
		{
			var shifts = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift);
			int openCount = shifts.Where(s => IsOpeningShift(s)).Count();
			int midCount = shifts.Where(s => IsMidshift(s)).Count();
			int closeCount = shifts.Where(s => IsClosingShift(s)).Count();

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
			var groupedDays = stats.Days.GroupBy(selector).OrderBy(g => g.Key).ToDictionary(g => g.Key);

			foreach (var kvp in groupedDays)
			{
				PrintSingleDayInfo(kvp.Key, kvp.Value);
			}
		}

		private static void PrintSingleDayInfo<T>(T key, IEnumerable<Day> days)
		{
			int totalCount = days.Count();
			int workingCount = days.Where(d => d.Shift != null).Count();

			var shiftsByType = days.Where(d => d.Shift != null).Select(d => d.Shift).GroupBy(s => GetShiftType(s)).ToDictionary(g => g.Key);
			int open = (shiftsByType.ContainsKey(ShiftType.Opening)) ? shiftsByType[ShiftType.Opening].Count() : 0;
			int mid = (shiftsByType.ContainsKey(ShiftType.Midshift)) ? shiftsByType[ShiftType.Midshift].Count() : 0;
			int close = (shiftsByType.ContainsKey(ShiftType.Closing)) ? shiftsByType[ShiftType.Closing].Count() : 0;

			Console.WriteLine($"{key.ToString()}: Worked {workingCount} of {totalCount} ({GetPercent(workingCount, totalCount)})");
			Console.WriteLine($"\t{open} opens ({GetPercent(open, workingCount)}), {mid} midshifts ({GetPercent(mid, workingCount)}), {close} closes ({GetPercent(close, workingCount)})");
		}

		private static string ProduceHourByHourInfo(StatsByDay stats)
		{
			char off = 'O';
			char pregame = 'P';
			char wait = '.';
			char work = 'W';

			int totalHours = (int)(stats.Days.Last().Date - stats.Days.First().Date).TotalHours;
			StringBuilder result = new StringBuilder();

			for (int overallHour = 0; overallHour <= totalHours; overallHour += 24)
			{
				int dayNumber = overallHour / 24;
				int hourInDay = overallHour % 24;

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
			int sum = onSum + offSum;

			double workingAverage = (double)onSum / stats.Weeks.Count;
			double offAverage = (double)offSum / stats.Weeks.Count;
			return new Tuple<double, double>(workingAverage, offAverage);
		}

		private static string GetAverageStartTime(StatsByDay stats)
		{
			var startTimes = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift.StartTime);
			return new SecondsAfterMidnight((startTimes.Sum(s => s.Seconds) / startTimes.Count())).ToTimeString();
		}

		private static string GetAverageEndTime(StatsByDay stats)
		{
			var endTimes = stats.Days.Where(d => d.Shift != null).Select(d => d.Shift.EndTime);
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
			// or, if it starts after 8am, ends before 3:30pm

			SecondsAfterMidnight fiveAM = new SecondsAfterMidnight(5, 0, 0);
			SecondsAfterMidnight eightAM = new SecondsAfterMidnight(8, 0, 0);
			SecondsAfterMidnight threeThirtyPM = new SecondsAfterMidnight(15, 30, 0);

			return (shift.StartTime >= fiveAM && shift.StartTime <= eightAM) || (shift.EndTime < threeThirtyPM);
		}

		private static bool IsMidshift(Shift shift)
		{
			// Midshift: ends on or before 8pm

			SecondsAfterMidnight eightPM = new SecondsAfterMidnight(20, 0, 0);

			return (shift.EndTime <= eightPM);
		}

		private static bool IsClosingShift(Shift shift)
		{
			// Closing shift: ends between 8pm and 11:59:59pm

			SecondsAfterMidnight eightPM = new SecondsAfterMidnight(20, 0, 0);
			SecondsAfterMidnight secondBeforeMidnight = new SecondsAfterMidnight(86399);

			return (shift.EndTime >= eightPM && shift.EndTime <= secondBeforeMidnight);
		}

		private static bool ShiftRequiredEarlyRise(Shift shift)
		{
			SecondsAfterMidnight waitStart = new SecondsAfterMidnight(shift.StartTime.Seconds - (int)(shift.WaitHours * 3600d));
			return (waitStart < new SecondsAfterMidnight(10, 0, 0));
		}

		private static SecondsAfterMidnight EstimateAlarm(Shift shift)
		{
			SecondsAfterMidnight waitStart = new SecondsAfterMidnight(shift.StartTime.Seconds - (int)(shift.WaitHours * 3600d));
			return new SecondsAfterMidnight(waitStart.Seconds - 3600);
		}

		private static IEnumerable<T> SortSeriesBy<T>(StatsByDay stats, Func<Series, T> selector) where T : IComparable<T>
		{
			return stats.Series.OrderBy(selector).Select(selector);
		}

		// Hour info
		private static HourType GetHourType(Day day, int hour)
		{
			if (day.Shift == null) { return HourType.Off; }
			else
			{
				SecondsAfterMidnight hourStart = new SecondsAfterMidnight(hour, 0, 0);
				SecondsAfterMidnight pregameStart;
				if (!ShiftRequiredEarlyRise(day.Shift)) { pregameStart = new SecondsAfterMidnight(day.Shift.StartTime.Seconds - 4500); }  // 75 minutes
				else { pregameStart = EstimateAlarm(day.Shift); }
				SecondsAfterMidnight waitStart = new SecondsAfterMidnight(day.Shift.StartTime.Seconds - (int)(day.Shift.WaitHours * 3600d));

				if (hourStart < pregameStart) { return HourType.Off; }
				else if (hourStart >= pregameStart && hourStart < waitStart) { return HourType.Pregame; }
				else if (hourStart >= waitStart && hourStart < day.Shift.StartTime) { return HourType.Wait; }
				else if (hourStart >= day.Shift.StartTime && hourStart < day.Shift.EndTime) { return HourType.Work; }
				else { return HourType.Off; }
			}
		}

		// Formatters
		private static string GetPercent(double a, double b)
		{
			return $"{(a / b) * 100:F2}%";
		}
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
