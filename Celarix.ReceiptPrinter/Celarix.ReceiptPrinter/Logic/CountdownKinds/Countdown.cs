using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.ReceiptPrinter.Logic.CountdownKinds
{
    internal abstract class Countdown
    {
        public abstract string Name(ZonedDateTime now);

        public abstract ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime);
        public abstract ZonedDateTime NextInstance(ZonedDateTime zonedDateTime);

        protected ZonedDateTime AtMidnight(ZonedDateTime zonedDateTime)
        {
            var localDateTime = zonedDateTime.Date.At(LocalTime.Midnight);
            return localDateTime.InZoneLeniently(zonedDateTime.Zone);
        }

        protected ZonedDateTime PreviousInstanceWithKnownDate(ZonedDateTime zonedDateTime, int countdownMonth, int countdownDay)
        {
            return PreviousInstanceWithKnownDate(zonedDateTime, GetPreviousCountdownYear(zonedDateTime, countdownMonth, countdownDay), countdownMonth, countdownDay);
        }

        protected ZonedDateTime PreviousInstanceWithKnownDate(ZonedDateTime zonedDateTime, int countdownYear, int countdownMonth, int countdownDay)
        {
            var dateAtMidnight = AtMidnight(zonedDateTime);
            if (dateAtMidnight.Year == countdownYear
                && dateAtMidnight.Month == countdownMonth
                && dateAtMidnight.Day == countdownDay)
            {
                // Today!
                return dateAtMidnight;
            }

            var previousOccurrenceLocalDate = new LocalDateTime(countdownYear, countdownMonth, countdownDay, 0, 0, 0);
            return previousOccurrenceLocalDate.InZoneLeniently(zonedDateTime.Zone);
        }

        protected ZonedDateTime NextInstanceWithKnownDate(ZonedDateTime zonedDateTime, int countdownMonth, int countdownDay)
        {
            return NextInstanceWithKnownDate(zonedDateTime, GetNextCountdownYear(zonedDateTime, countdownMonth, countdownDay), countdownMonth, countdownDay);
        }

        protected ZonedDateTime NextInstanceWithKnownDate(ZonedDateTime zonedDateTime, int countdownYear, int countdownMonth, int countdownDay)
        {
            var dateAtMidnight = AtMidnight(zonedDateTime);
            if (dateAtMidnight.Year == countdownYear
                && dateAtMidnight.Month == countdownMonth
                && dateAtMidnight.Day == countdownDay)
            {
                // Today!
                return dateAtMidnight;
            }

            var nextOccurrenceLocalDate = new LocalDateTime(countdownYear, countdownMonth, countdownDay, 0, 0, 0);
            return nextOccurrenceLocalDate.InZoneLeniently(zonedDateTime.Zone);
        }

        protected int GetPreviousCountdownYear(ZonedDateTime zonedDateTime, int countdownMonth, int countdownDay)
        {
            return zonedDateTime.Month > countdownMonth || (zonedDateTime.Month == countdownMonth && zonedDateTime.Day > countdownDay)
                ? zonedDateTime.Year
                : zonedDateTime.Year - 1;
        }

        protected int GetNextCountdownYear(ZonedDateTime zonedDateTime, int countdownMonth, int countdownDay)
        {
            return zonedDateTime.Month < countdownMonth || (zonedDateTime.Month == countdownMonth && zonedDateTime.Day < countdownDay)
                ? zonedDateTime.Year
                : zonedDateTime.Year + 1;
        }
    }
}
