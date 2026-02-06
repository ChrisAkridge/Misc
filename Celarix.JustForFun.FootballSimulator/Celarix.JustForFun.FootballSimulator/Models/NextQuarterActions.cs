using Celarix.JustForFun.FootballSimulator.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class NextQuarterActions(int currentPeriodNumber)
    {
        public int NextPeriodNumber { get; init; } = currentPeriodNumber + 1;
        public int DurationInSeconds => (NextPeriodNumber <= 4)
            ? Constants.SecondsPerQuarter
            : Constants.SecondsPerOvertimePeriod;
        public bool CoinTossNeeded => (NextPeriodNumber % 4) == 1;
        public bool CoinTossLoserReceivesPossession => (NextPeriodNumber % 4) == 3;
        public bool CurrentDriveEnds => CoinTossNeeded || CoinTossLoserReceivesPossession;
    }
}
