using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown
{
    internal sealed class RealizedCountdown
    {
        public required string Name { get; set; }
        public ZonedDateTime? PreviousOccurence { get; set; }
        public ZonedDateTime NextOccurence { get; set; }

        public Duration DurationUntil(ZonedDateTime now)
        {
            // Calculate the duration until the next occurrence
            var durationUntilNext = NextOccurence - now;
            return durationUntilNext;
        }

        public Duration? DurationSince(ZonedDateTime now)
        {
            // Calculate the duration since the previous occurrence
            var durationSincePrevious = now - PreviousOccurence;
            return durationSincePrevious;
        }

        public double DecibelSecondsUntil(ZonedDateTime now)
        {
            var totalSecondsUntil = DurationUntil(now).TotalSeconds;
            if (totalSecondsUntil <= 0)
            {
                return double.NegativeInfinity;
            }
            var log10 = Math.Log10(totalSecondsUntil);
            return 10d * log10;
        }
    }
}
