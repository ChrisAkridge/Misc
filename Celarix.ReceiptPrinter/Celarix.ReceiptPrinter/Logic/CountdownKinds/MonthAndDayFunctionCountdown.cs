using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.ReceiptPrinter.Logic.CountdownKinds
{
    internal sealed class MonthAndDayFunctionCountdown : Countdown
    {
        private readonly string name;
        private readonly Func<int, (int Month, int Day)> getMonthAndDay;

        public MonthAndDayFunctionCountdown(string name, Func<int, (int Month, int Day)> getMonthAndDay)
        {
            this.name = name;
            this.getMonthAndDay = getMonthAndDay;
        }

        public override string Name(ZonedDateTime now) => name;

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            var monthAndDay = getMonthAndDay(zonedDateTime.Year);
            return NextInstanceWithKnownDate(zonedDateTime, monthAndDay.Month, monthAndDay.Day);
        }

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            var monthAndDay = getMonthAndDay(zonedDateTime.Year);
            return PreviousInstanceWithKnownDate(zonedDateTime, monthAndDay.Month, monthAndDay.Day);
        }
    }
}
