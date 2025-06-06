using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal sealed class LeapDayCountdown : Countdown
    {
        public override string Name(ZonedDateTime now) => "Leap Day";

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            var year = zonedDateTime.Year;
            while (!CalendarSystem.Gregorian.IsLeapYear(year))
            {
                year -= 1;
            }
            return PreviousInstanceWithKnownDate(zonedDateTime, year, 2, 29);
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime)
        {
            var year = zonedDateTime.Year;
            while (!CalendarSystem.Gregorian.IsLeapYear(year))
            {
                year += 1;
            }
            return NextInstanceWithKnownDate(zonedDateTime, year, 2, 29);
        }
    }
}
