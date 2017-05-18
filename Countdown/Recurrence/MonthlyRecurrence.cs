using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown.Recurrence
{
	internal sealed class MonthlyRecurrence : IRecurrence
	{
		public int OccursOnDayNumber { get; }
		public int OccursOnNthWeekday { get; }
		public IsoDayOfWeek OccursOnWeekday { get; }

		public bool DayNumberOrWeekdayNumber { get; }

		public MonthlyRecurrence(int occursOnDayNumber)
		{
			DayNumberOrWeekdayNumber = true;
			OccursOnDayNumber = occursOnDayNumber;
		}

		public MonthlyRecurrence(int occursOnNthWeekday, IsoDayOfWeek occursOnWeekday)
		{
			DayNumberOrWeekdayNumber = false;
			OccursOnNthWeekday = occursOnNthWeekday;
			OccursOnWeekday = occursOnWeekday;
		}

		public Instant RescheduleAt(Event oldEvent)
		{
			return oldEvent.EndTime + Duration.FromDays(1);
		}

		public Instant RescheduleFor(Event oldEvent)
		{
			return RescheduleForImpl(oldEvent.EndTime.InZone(
				DateTimeZoneProviders.Tzdb["America/New_York"],
				CalendarSystem.Iso));
		}

		private Instant RescheduleForImpl(ZonedDateTime endTimeZDT)
		{
			if (DayNumberOrWeekdayNumber)
			{
				int difference = OccursOnDayNumber - endTimeZDT.Day;
				endTimeZDT = AddOneMonth(endTimeZDT);
				return endTimeZDT.PlusHours(difference * 24).ToInstant();
			}
			else
			{
				// http://stackoverflow.com/a/288542/2709212
				endTimeZDT = AddOneMonth(endTimeZDT);
				int nthWeekday = OccursOnWeekday.IsoWeekdayToInt();
				var date = new LocalDate(endTimeZDT.Year, endTimeZDT.Month, 1);

				while (date.DayOfWeek.IsoWeekdayToInt() != nthWeekday)
				{
					date = date.PlusDays(1);
				}

				if (date.Month != endTimeZDT.Month)
				{
					// Pick a month with an nth weekday. Recursion should find one eventually.
					return RescheduleForImpl(AddOneMonth(endTimeZDT));
				}

				date = date.PlusDays((OccursOnNthWeekday - 1) * 7);

				var localDateTime = new LocalDateTime(date.Year, date.Month, date.Day,
					endTimeZDT.Hour, endTimeZDT.Minute, CalendarSystem.Iso);
				return localDateTime.ToZonedDateTime().ToInstant();
			}
		}

		private ZonedDateTime AddOneMonth(ZonedDateTime zdt)
		{
			var nextMonthLDT = new LocalDateTime(zdt.Year, zdt.Month,
						zdt.Day, zdt.Hour, zdt.Minute, CalendarSystem.Iso);
			nextMonthLDT = nextMonthLDT + Period.FromMonths(1);
			var timeZone = DateTimeZoneProviders.Tzdb["America/New_York"];
			return timeZone.AtStrictly(nextMonthLDT);
		}
	}
}
