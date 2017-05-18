using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown.Recurrence
{
	internal sealed class WeeklyRecurrence : IRecurrence
	{
		public bool[] WeekdayFlags { get; }

		public WeeklyRecurrence(IEnumerable<bool> weekdayFlags)
		{
			WeekdayFlags = weekdayFlags.ToArray();
		}

		public string GetWeekdayFlagsString()
		{
			char[] flags = new char[7];
			for (int i = 0; i < 7; i++)
			{
				// E for enabled, D for disabled
				flags[i] = WeekdayFlags[i] ? 'E' : 'D';
			}
			return new string(flags);
		}

		public Instant RescheduleAt(Event oldEvent)
		{
			return oldEvent.EndTime + Duration.FromSeconds(1);
		}

		public Instant RescheduleFor(Event oldEvent)
		{
			var endTimeZDT = oldEvent.EndTime.InZone(DateTimeZoneProviders.Tzdb["America/New_York"],
				CalendarSystem.Iso);
			var endTimeDayOfWeek = endTimeZDT.DayOfWeek;
			var intDayOfWeek = endTimeZDT.DayOfWeek.IsoWeekdayToInt();
			var nextDayOfWeek = WeekdayFlags.FindIndexOfNextFlag(intDayOfWeek);

			int difference;
			if (intDayOfWeek == nextDayOfWeek) { difference = 7; }
			else if (nextDayOfWeek < intDayOfWeek) { difference = (nextDayOfWeek + 7) - intDayOfWeek; }
			else { difference = nextDayOfWeek - intDayOfWeek; }

			return oldEvent.EndTime + Duration.FromDays(difference);
		}
	}
}
