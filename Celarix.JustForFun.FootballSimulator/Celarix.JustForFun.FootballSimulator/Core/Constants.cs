using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal static class Constants
    {
        public const double ChaosMultiplierChance = 1 / 10000;
        public const double ChaosMultiplier = 1000;

        public const int SecondsPerQuarter = 15 * 60;
        public const int SecondsPerOvertimePeriod = 10 * 60;
        public const double LeftSidelineCross = -(80d / 3d);
        public const double RightSidelineCross = 80d / 3d;
        public const double HomeEndLineYard = -10d;
        public const double AwayEndLineYard = 110d;
        public const double HomeGoalLineYard = 0d;
        public const double AwayGoalLineYard = 100d;
    }
}
