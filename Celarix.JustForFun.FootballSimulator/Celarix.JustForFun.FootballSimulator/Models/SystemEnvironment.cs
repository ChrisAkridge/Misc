using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class SystemEnvironment
    {
        public required FootballContext FootballContext { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public required PlayerFactory PlayerFactory { get; init; }
        public ScheduleGenerator3? ScheduleGenerator { get; set; }
        public DivisionTiebreaker? DivisionTiebreaker { get; set; }
        public GameRecord? CurrentGameRecord { get; set; }
        public GameContext? CurrentGameContext { get; set; }
    }
}
