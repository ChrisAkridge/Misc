using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.ReceiptPrinter.Logic
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
    }
}
