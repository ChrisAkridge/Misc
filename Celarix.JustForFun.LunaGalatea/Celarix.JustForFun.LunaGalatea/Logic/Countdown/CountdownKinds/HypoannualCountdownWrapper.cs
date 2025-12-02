using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal sealed class HypoannualCountdownWrapper : Countdown
    {
        private readonly Countdown wrappedCountdown;
        private readonly int exampleYearContainingOccurrence;
        private readonly int yearsBetweenOccurrences;

        public HypoannualCountdownWrapper(Countdown wrappedCountdown, int exampleYearContainingOccurrence, int yearsBetweenOccurrences)
        {
            this.wrappedCountdown = wrappedCountdown;
            this.exampleYearContainingOccurrence = exampleYearContainingOccurrence;
            this.yearsBetweenOccurrences = yearsBetweenOccurrences;
        }

        public override string Name(ZonedDateTime now) => wrappedCountdown.Name(now);

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            ZonedDateTime? previousInstance = wrappedCountdown.PreviousInstance(GetDateInYear(zonedDateTime, exampleYearContainingOccurrence));
            ZonedDateTime? secondToLastInstance = null;

            if (previousInstance == null)
            {
                return null;
            }

            var comparer = ZonedDateTime.Comparer.Instant;
            int year = exampleYearContainingOccurrence;
            while (comparer.Compare(previousInstance!.Value, zonedDateTime) < 0)
            {
                year += yearsBetweenOccurrences;
                secondToLastInstance = previousInstance;
                previousInstance = wrappedCountdown.PreviousInstance(GetDateInYear(zonedDateTime, year));
            }
            return secondToLastInstance;
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            ZonedDateTime nextInstance = wrappedCountdown.NextInstance(GetJanuary1InYear(zonedDateTime, exampleYearContainingOccurrence));
            var comparer = ZonedDateTime.Comparer.Instant;
            int year = exampleYearContainingOccurrence;
            while (comparer.Compare(nextInstance, zonedDateTime) <= 0)
            {
                year += yearsBetweenOccurrences;
                nextInstance = wrappedCountdown.NextInstance(GetJanuary1InYear(zonedDateTime, year));
            }
            return nextInstance;
        }

        private ZonedDateTime GetDateInYear(ZonedDateTime zonedDateTime, int year)
        {
            if (zonedDateTime.Day == 29 && zonedDateTime.Month == 2 && !DateTime.IsLeapYear(year))
            {
                // Handle leap day by going to Mar 1 in non-leap years.
                return new LocalDateTime(year, 3, 1, 0, 0, 0).InZoneLeniently(zonedDateTime.Zone);
            }

            return new LocalDateTime(year, zonedDateTime.Month, zonedDateTime.Day, 0, 0, 0).InZoneLeniently(zonedDateTime.Zone);
        }

        private ZonedDateTime GetJanuary1InYear(ZonedDateTime zonedDateTime, int year)
        {
            return new LocalDateTime(year, 1, 1, 0, 0, 0).InZoneLeniently(zonedDateTime.Zone);
        }
    }
}
