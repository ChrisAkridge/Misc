using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown.Recurrence
{
	internal sealed class YearlyRecurrence : IRecurrence
	{
		public LocalDate DayInYear { get; }

		public YearlyRecurrence(int month, int day)
		{
			DayInYear = new LocalDate(2000, month, day);
		}

		public Instant RescheduleAt(Event oldEvent)
		{
			return oldEvent.EndTime + Duration.FromDays(1);
		}

		public Instant RescheduleFor(Event oldEvent)
		{
			var endTimeZDT = oldEvent.EndTime.InZone(DateTimeZoneProviders.Tzdb["America/New_York"]);
			var localDateTime = new LocalDateTime(endTimeZDT.Year, DayInYear.Month, DayInYear.Day,
				endTimeZDT.Hour, endTimeZDT.Minute, CalendarSystem.Iso);
			localDateTime = localDateTime.PlusYears(1);
			return localDateTime.ToZonedDateTime().ToInstant();
		}
	}
}
