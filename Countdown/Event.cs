using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Countdown.Recurrence;
using NodaTime;

namespace Countdown
{
	internal sealed class Event
	{
		public string Name { get; }
		public Instant? StartTime { get; }
		public Instant EndTime { get; }
		public IRecurrence Recurrence { get; }

		public Event(string name, Instant? startTime, Instant endTime, IRecurrence recurrence)
		{
			Name = name;
			StartTime = startTime;
			EndTime = endTime;
			Recurrence = recurrence;
		}

		public Duration GetTimeUntil(Instant instant)
		{
			return EndTime - instant;
		}

		public string FormatRemainingTime(Instant instant, TimeLeftForm timeLeftForm, int decimalPlaces, 
			bool prependName = true)
		{
			var timeUntil = GetTimeUntil(instant);
			string prefix = (prependName) ? (Name + ": ") : "";
			string suffix = null;

			if (timeLeftForm == TimeLeftForm.Default)
			{
				suffix = DurationFormatter.AsDefault(timeUntil);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingSeconds)
			{
				suffix = DurationFormatter.AsRemainingSeconds(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingMinutes)
			{
				suffix = DurationFormatter.AsRemainingMinutes(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingHours)
			{
				suffix = DurationFormatter.AsRemainingHours(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingDays)
			{
				suffix = DurationFormatter.AsRemainingDays(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingWeeks)
			{
				suffix = DurationFormatter.AsRemainingWeeks(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.RemainingYears)
			{
				suffix = DurationFormatter.AsRemainingYears(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.DecibelsSeconds)
			{
				suffix = DurationFormatter.AsDecibelsSeconds(timeUntil, decimalPlaces);
			}
			else if (timeLeftForm == TimeLeftForm.XKCD1017Equation)
			{
				if (!StartTime.HasValue) { return prefix + " --- "; }
				suffix = DurationFormatter.AsXKCD1017Equation(StartTime.Value, EndTime,
					SystemClock.Instance.GetCurrentInstant(), decimalPlaces);
			}
			return prefix + suffix;
		}
	}
}
