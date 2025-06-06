using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal class NextPowerOf2And10Countdown : Countdown
    {
        private const int JeanneCalmentAgeInDays = 44724;
        private readonly List<(string Name, ZonedDateTime occurrence)> powerOf2And10Occurrences = [];
        private readonly string eventName;

        public NextPowerOf2And10Countdown(DateTimeZone timeZone, string eventName, int eventYear, int eventMonth, int eventDay, int eventHour, int eventMinute, int eventSecond)
        {
            var eventInstant = new LocalDateTime(eventYear, eventMonth, eventDay, eventHour, eventMinute, eventSecond)
                .InZoneLeniently(timeZone)
                .ToInstant();
            var maximumInstant = eventInstant + Duration.FromDays(JeanneCalmentAgeInDays);
            this.eventName = eventName;

            Duration currentDuration;
            Instant currentInstant;

            var durationFuncDictionary = new Dictionary<string, Func<double, Duration>>
            {
                { "nanoseconds", Duration.FromNanoseconds },
                { "milliseconds", Duration.FromMilliseconds },
                { "seconds", Duration.FromSeconds },
                { "minutes", Duration.FromMinutes },
                { "hours", Duration.FromHours },
                { "days", Duration.FromDays },
            };

            foreach (var kvp in durationFuncDictionary)
            {
                // Powers of 2
                var currentPowerOf2 = 0;
                currentInstant = eventInstant;

                while (currentInstant < maximumInstant)
                {
                    currentDuration = kvp.Value(Math.Pow(2, currentPowerOf2));
                    currentInstant += currentDuration;
                    powerOf2And10Occurrences.Add(($"2^{currentPowerOf2} {kvp.Key}", new ZonedDateTime(currentInstant, timeZone)));
                    currentPowerOf2 += 1;
                }

                // Powers of 10
                var currentPowerOf10 = 0;
                currentInstant = eventInstant;

                while (currentInstant < maximumInstant)
                {
                    currentDuration = kvp.Value(Math.Pow(10, currentPowerOf10));
                    currentInstant += currentDuration;
                    powerOf2And10Occurrences.Add(($"10^{currentPowerOf10} {kvp.Key}", new ZonedDateTime(currentInstant, timeZone)));
                    currentPowerOf10 += 1;
                }
            }

            powerOf2And10Occurrences = [.. powerOf2And10Occurrences.OrderBy(x => x.occurrence.ToInstant())];
        }

        public override string Name(ZonedDateTime now)
        {
            foreach (var occurrence in powerOf2And10Occurrences)
            {
                // Convert the time zone in occurrence to the one in now
                // (i.e. 2:00pm MDT becomes 4:00pm EDT)
                var occurrenceZonedDateTime = occurrence.occurrence.WithZone(now.Zone);
                if (ZonedDateTime.Comparer.Instant.Compare(occurrenceZonedDateTime, now) > 0)
                {
                    return $"{occurrence.Name} since {eventName}";
                }
            }

            return eventName;
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            foreach (var occurrence in powerOf2And10Occurrences)
            {
                // Convert the time zone in occurrence to the one in zonedDateTime
                // (i.e. 2:00pm MDT becomes 4:00pm EDT)
                var occurrenceZonedDateTime = occurrence.occurrence.WithZone(zonedDateTime.Zone);
                if (ZonedDateTime.Comparer.Instant.Compare(occurrenceZonedDateTime, zonedDateTime) > 0)
                {
                    return occurrenceZonedDateTime;
                }
            }

            // If no future occurrence is found, I am officially the oldest person in human history
            throw new ArgumentOutOfRangeException("How does it feel to be older than Jeanne Calment?");
        }

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            foreach (var occurrence in Enumerable.Reverse(powerOf2And10Occurrences))
            {
                // Convert the time zone in occurrence to the one in zonedDateTime
                // (i.e. 2:00pm MDT becomes 4:00pm EDT)
                var occurrenceZonedDateTime = occurrence.occurrence.WithZone(zonedDateTime.Zone);
                if (ZonedDateTime.Comparer.Instant.Compare(occurrenceZonedDateTime, zonedDateTime) < 0)
                {
                    return occurrenceZonedDateTime;
                }
            }

            // If no previous occurrence is found, you must have asked for a point before I was born
            throw new ArgumentOutOfRangeException("Too soon");
        }
    }
}
