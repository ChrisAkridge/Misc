using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.ReceiptPrinter.Logic.CountdownKinds
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
            var previousYearWithOccurrence = GetPreviousYearWithOccurrence(zonedDateTime.Year);
            return wrappedCountdown.PreviousInstance(GetJanuary1stMidnightOfYear(zonedDateTime.Zone, previousYearWithOccurrence));
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            var nextYearWithOccurrence = GetNextYearWithOccurrence(zonedDateTime.Year);
            return wrappedCountdown.NextInstance(GetJanuary1stMidnightOfYear(zonedDateTime.Zone, nextYearWithOccurrence));
        }

        private int GetPreviousYearWithOccurrence(int year)
        {
            if (IsOccurrenceYear(year))
            {
                return year;
            }

            int yearsSinceExampleYear = year - exampleYearContainingOccurrence;
            int offset = yearsSinceExampleYear % yearsBetweenOccurrences;
            if (offset < 0)
            {
                offset += yearsBetweenOccurrences;
            }
            return year - offset;
        }

        private int GetNextYearWithOccurrence(int year)
        {
            if (IsOccurrenceYear(year))
            {
                return year;
            }

            int yearsSinceExampleYear = year - exampleYearContainingOccurrence;
            int offset = yearsSinceExampleYear % yearsBetweenOccurrences;
            if (offset < 0)
            {
                offset += yearsBetweenOccurrences;
            }
            return year + (yearsBetweenOccurrences - offset);
        }

        private bool IsOccurrenceYear(int year)
        {
            var yearsSinceExampleYear = year - exampleYearContainingOccurrence;
            return yearsSinceExampleYear % yearsBetweenOccurrences == 0;
        }

        private ZonedDateTime GetJanuary1stMidnightOfYear(DateTimeZone zone, int year)
        {
            var date = new LocalDateTime(year, 1, 1, 0, 0, 0);
            return date.InZoneLeniently(zone);
        }
    }
}
