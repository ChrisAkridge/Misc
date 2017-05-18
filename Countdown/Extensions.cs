using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Countdown.Recurrence;
using NodaTime;

namespace Countdown
{
	internal static class Extensions
	{
		public static int IsoWeekdayToInt(this IsoDayOfWeek dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case IsoDayOfWeek.Monday:
					return 1;
				case IsoDayOfWeek.Tuesday:
					return 2;
				case IsoDayOfWeek.Wednesday:
					return 3;
				case IsoDayOfWeek.Thursday:
					return 4;
				case IsoDayOfWeek.Friday:
					return 5;
				case IsoDayOfWeek.Saturday:
					return 6;
				case IsoDayOfWeek.Sunday:
					return 0;
				case IsoDayOfWeek.None:
				default:
					return -1;
			}
		}

		public static IsoDayOfWeek ToIsoWeekday(this int dayOfWeek)
		{
			switch (dayOfWeek)
			{
				case 0: return IsoDayOfWeek.Sunday;
				case 1: return IsoDayOfWeek.Monday;
				case 2: return IsoDayOfWeek.Tuesday;
				case 3: return IsoDayOfWeek.Wednesday;
				case 4: return IsoDayOfWeek.Thursday;
				case 5: return IsoDayOfWeek.Friday;
				case 6: return IsoDayOfWeek.Saturday;
				default: return IsoDayOfWeek.None;
			}
		}

		public static int IncrementInRange(this int value, int min, int max)
		{
			if (value == max) { return min; }
			else { return value += 1; }
		}

		public static int FindIndexOfNextFlag(this IList<bool> flags, int index)
		{
			int i = index.IncrementInRange(0, flags.Count - 1);
			while (!flags[i])
			{
				i = i.IncrementInRange(0, flags.Count - 1);
			}
			return i;
		}

		public static ZonedDateTime ToZonedDateTime(this LocalDateTime ldt)
		{
			var zone = DateTimeZoneProviders.Tzdb["America/New_York"];
			return zone.AtLeniently(ldt);
		}

		public static RecurrenceType GetRecurrenceType(this IRecurrence recurrence)
		{
			if (recurrence is DailyRecurrence) { return RecurrenceType.DailyRecurrence; }
			else if (recurrence is WeeklyRecurrence) { return RecurrenceType.WeeklyRecurrence; }
			else if (recurrence is MonthlyRecurrence) { return RecurrenceType.MonthlyRecurrence; }
			else if (recurrence is YearlyRecurrence) { return RecurrenceType.YearlyRecurrence; }
			else { throw new InvalidOperationException(); }
		}
	}
}
