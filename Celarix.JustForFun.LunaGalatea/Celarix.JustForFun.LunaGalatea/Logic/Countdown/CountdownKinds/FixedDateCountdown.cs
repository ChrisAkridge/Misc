using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal sealed class FixedDateCountdown : Countdown
    {
        private readonly string name;

        public int Month { get; init; }
        public int Day { get; init; }

        public FixedDateCountdown(string name, int month, int day)
        {
            this.name = name;
            Month = month;
            Day = day;
        }

        public override string Name(ZonedDateTime now) => name;

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime) => PreviousInstanceWithKnownDate(zonedDateTime, Month, Day);

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime) => NextInstanceWithKnownDate(zonedDateTime, Month, Day);
    }
}
