using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Gameplay
{
    internal sealed class GameClock
    {
        private const int SecondsInRegulationQuarter = 15 * 60;

        public int PeriodNumber { get; private set; } = 1;
        public int SecondsLeftInPeriod { get; private set; } = SecondsInRegulationQuarter;
        public int SecondsElapsedInPeriod { get; private set; } = 0;
        public int HomeTeamTimeouts { get; private set; } = 3;
        public int AwayTeamTimeouts { get; private set; } = 3;
        public ClockEvent LastClockEvent { get; private set; } = ClockEvent.NewCoinTossRequired;

        public void Advance(int secondsElapsed)
        {
            if (LastClockEvent == ClockEvent.NewCoinTossRequired)
            {
                LastClockEvent = ClockEvent.TimeElapsed;
            }
        }
    }
}
