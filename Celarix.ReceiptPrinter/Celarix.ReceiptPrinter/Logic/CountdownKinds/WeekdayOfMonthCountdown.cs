using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.ReceiptPrinter.Logic.CountdownKinds
{
    internal sealed class WeekdayOfMonthCountdown : Countdown
    {
        private string name;

        public int Month { get; init; }
        public IsoDayOfWeek DayOfWeek { get; init; }
        public int WeekOfMonth { get; init; }

        public WeekdayOfMonthCountdown(string name, int month, IsoDayOfWeek dayOfWeek, int weekOfMonth)
        {
            this.name = name;
            Month = month;
            DayOfWeek = dayOfWeek;
            WeekOfMonth = weekOfMonth;
        }

        public override string Name(ZonedDateTime now) => name;

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            var occurrenceDateForThisYear = OccurrenceDateForYear(zonedDateTime.Year);
            var occurrenceDateForLastYear = OccurrenceDateForYear(zonedDateTime.Year - 1);

            var occurrenceHasPassedThisYear = occurrenceDateForThisYear < zonedDateTime.Date;

            if (occurrenceHasPassedThisYear)
            {
                return PreviousInstanceWithKnownDate(zonedDateTime, Month, occurrenceDateForThisYear.Day);
            }
            else
            {
                return PreviousInstanceWithKnownDate(zonedDateTime, Month, occurrenceDateForLastYear.Day);
            }
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            var occurrenceDateForThisYear = OccurrenceDateForYear(zonedDateTime.Year);
            var occurrenceDateForNextYear = OccurrenceDateForYear(zonedDateTime.Year + 1);

            var occurrenceHasPassedThisYear = occurrenceDateForThisYear < zonedDateTime.Date;

            if (occurrenceHasPassedThisYear)
            {
                return NextInstanceWithKnownDate(zonedDateTime, Month, occurrenceDateForNextYear.Day);
            }
            else
            {
                return NextInstanceWithKnownDate(zonedDateTime, Month, occurrenceDateForThisYear.Day);
            }
        }

        private LocalDate OccurrenceDateForYear(int year)
        {
            return LocalDate.FromYearMonthWeekAndDay(year, Month, WeekOfMonth, DayOfWeek);
        }
    }
}
