using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Standings;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using Celarix.JustForFun.FootballSimulator.Tiebreaking;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class SystemEnvironment
    {
        public required IFootballRepository FootballRepository { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public required PlayerFactory PlayerFactory { get; init; }
        public required ISummaryWriter SummaryWriter { get; init; }
        public required IDebugContextWriter DebugContextWriter { get; init; }
        public ScheduleGenerator3? ScheduleGenerator { get; set; }
        public TeamRanker? TeamRanker { get; set; }
        public GameRecord? CurrentGameRecord { get; set; }
        public GameContext? CurrentGameContext { get; set; }
    }
}
