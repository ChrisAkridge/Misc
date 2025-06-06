using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Celarix.JustForFun.LunaGalatea.Logic.Countdown.CountdownKinds
{
    internal class NumberedAnniversaryCountdownWrapper : Countdown
    {
        private readonly Countdown wrappedCountdown;
        private readonly string name;

        public int BirthYear { get; init; }
        public NumberedAnniversaryKind NumberedAnniversaryKind { get; init; }

        public NumberedAnniversaryCountdownWrapper(string name,
            Countdown wrappedCountdown,
            int firstOccurrenceYear,
            NumberedAnniversaryKind numberedAnniversaryKind)
        {
            this.name = name;
            this.wrappedCountdown = wrappedCountdown;
            BirthYear = firstOccurrenceYear;
            NumberedAnniversaryKind = numberedAnniversaryKind;
        }

        public override string Name(ZonedDateTime now)
        {
            var nextInstance = NextInstance(now);
            var instanceNumber = nextInstance.Year - BirthYear;

            return NumberedAnniversaryKind switch
            {
                NumberedAnniversaryKind.Birthday => $"{name}'s {instanceNumber.WithOrdinal()} Birthday",
                NumberedAnniversaryKind.Anniversary => $"{instanceNumber.WithOrdinal()} Anniversary of {name}",
                NumberedAnniversaryKind.RomanNumeral => $"{name} {instanceNumber.ToRomanNumerals()}",
                _ => throw new ArgumentOutOfRangeException(nameof(NumberedAnniversaryKind), NumberedAnniversaryKind, null)
            };
        }

        public override ZonedDateTime? PreviousInstance(ZonedDateTime zonedDateTime) => wrappedCountdown.PreviousInstance(zonedDateTime);

        public override ZonedDateTime NextInstance(ZonedDateTime zonedDateTime) => wrappedCountdown.NextInstance(zonedDateTime);
    }
}
