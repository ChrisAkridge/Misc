using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeijerStatAnalyzer
{
	public sealed class StatsByDay
	{
		private List<Day> days = new List<Day>();
		private List<Week> weeks;
		private List<Series> series;

		public IReadOnlyList<Day> Days => days.AsReadOnly();
		public IReadOnlyList<Week> Weeks => weeks.AsReadOnly();
		public IReadOnlyList<Series> Series => series.AsReadOnly();

		public double TotalWorkingDays => Series.Sum(s => s.Days.Count(d => d.Shift.StartTime > SecondsAfterMidnight.Midnight));
		public double TotalDaysOff => Weeks.Sum(w => w.DaysOff);

		public double TotalRecordedHours => days.Count * 24d;
		public double TotalTimeAtMeijer { get; private set; }
		public double TotalScheduledTime { get; private set; }
		public double TotalWorkingHours { get; private set; }
		public double TotalWorkingQuarters => TotalWorkingHours * 4d;
		public double TotalWaitingHours { get; private set; }
		public double TotalPaidHours { get; private set; }
		public double TotalUnpaidHours { get; private set; }
		public double TotalPay { get; private set; }

		public StatsByDay()
		{
			days.Add(new Day(new DateTime(2013, 9, 15), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2013, 9, 16), null, null, null, null, DayType.Off));

			days.Add(new Day(new DateTime(2018, 5, 21), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2018, 5, 22), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2018, 5, 23), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2018, 5, 24), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2018, 5, 25), null, null, null, null, DayType.Off));
			days.Add(new Day(new DateTime(2018, 5, 26), null, null, null, null, DayType.Off));
		}

		public void AddDay(Day day) => days.Add(day);

		public void FinalConstruction()
		{
			days = days.OrderBy(d => d.Date).ToList();

			weeks = new List<Week>();

			int i = 0;
			while (i < days.Count)
			{
				Day sunday = days[i];
				Day monday = days[i + 1];
				Day tuesday = days[i + 2];
				Day wednesday = days[i + 3];
				Day thursday = days[i + 4];
				Day friday = days[i + 5];
				Day saturday = days[i + 6];

				weeks.Add(new Week(sunday.Date, sunday, monday, tuesday, wednesday, thursday, friday, saturday));
				i += 7;
			}

			List<Shift> shifts = days.Where(d => d.Shift != null).Select(d => d.Shift).ToList();
			TotalTimeAtMeijer = shifts.Sum(s => s.TimeAtMeijer);
			TotalScheduledTime = shifts.Sum(s => s.ShiftLength.TotalHours);
			TotalWorkingHours = shifts.Sum(s => s.WorkedHours);
			TotalWaitingHours = shifts.Sum(s => s.WaitHours);
			TotalPaidHours = shifts.Sum(s => s.PaidHours);
			TotalUnpaidHours = shifts.Sum(s => s.UnpaidHours);
			TotalPay = shifts.Sum(s => s.EstimatedFinalPay);

			SetDayNumbersOfType(days);
			series = MakeSeries(days);
		}

		private static void SetDayNumbersOfType(List<Day> days)
		{
			int workingShifts = 0;
			int nonWorkingShifts = 0;
			int paidDaysOff = 0;
			int unpaidDaysOff = 0;

			foreach (Day day in days)
			{
				if (day.Date.Year == 2013 && day.Date.Month == 9)
				{
					switch (day.Date.Day)
					{
						case 15:
							day.DayNumberOfType = -1;
							continue;
						case 16:
							day.DayNumberOfType = 0;
							continue;
						default:
							break;
					}
				}

				switch (day.Type)
				{
					case DayType.Working:
						day.DayNumberOfType = ++workingShifts;
						break;
					case DayType.Off:
						day.DayNumberOfType = ++nonWorkingShifts;
						break;
					case DayType.PaidTimeOff:
						day.DayNumberOfType = ++paidDaysOff;
						break;
					case DayType.UnpaidTimeOff:
						day.DayNumberOfType = ++unpaidDaysOff;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static List<Series> MakeSeries(IEnumerable<Day> days)
		{
			var daysTaken = new List<Day>();
			var result = new List<Series>();

			foreach (Day day in days)
			{
				if (day.Shift != null) { daysTaken.Add(day); }
				else if (daysTaken.Any())
				{
					result.Add(new Series(daysTaken));
					daysTaken = new List<Day>();
				}
			}

			return result;
		}
	}
}
