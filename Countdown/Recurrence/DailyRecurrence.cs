using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Countdown.Recurrence
{
	internal sealed class DailyRecurrence : IRecurrence
	{
		public int RepeatEveryNDays { get; }

		public DailyRecurrence(int repeatEveryNDays)
		{
			RepeatEveryNDays = repeatEveryNDays;
		}

		public Instant RescheduleAt(Event oldEvent)
		{
			// Daily events are rescheduled one second after the event ends.
			return oldEvent.EndTime + Duration.FromSeconds(1L);
		}

		public Instant RescheduleFor(Event oldEvent)
		{
			// Add n days to the end time of the new event.
			return oldEvent.EndTime + Duration.FromDays(RepeatEveryNDays);
		}
	}
}
