using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal sealed class OffsetCountdownWrapper : Countdown
    {
        private readonly Countdown wrappedCountdown;
        private readonly Duration offset;
        public OffsetCountdownWrapper(Countdown wrappedCountdown, Duration offset)
        {
            this.wrappedCountdown = wrappedCountdown;
            this.offset = offset;
        }
        public override string Name(ZonedDateTime now) => wrappedCountdown.Name(now);
        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime)
        {
            ZonedDateTime? previousInstance = wrappedCountdown.PreviousInstance(zonedDateTime);
            if (previousInstance == null) { return null; }
            return previousInstance.Value.Plus(offset);
        }

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime) =>
            wrappedCountdown.NextInstance(zonedDateTime).Plus(offset);
    }
}
