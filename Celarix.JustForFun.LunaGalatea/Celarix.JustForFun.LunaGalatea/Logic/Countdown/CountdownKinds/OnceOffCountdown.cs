using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal sealed class OnceOffCountdown : Countdown
    {
        private readonly string name;
        private readonly ZonedDateTime occurrenceDateTime;

        public OnceOffCountdown(string name, ZonedDateTime occurrenceDateTime)
        {
            this.name = name;
            this.occurrenceDateTime = occurrenceDateTime;
        }

        public override string Name(ZonedDateTime now) => name;

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime) => occurrenceDateTime;

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime) => null;
    }
}
