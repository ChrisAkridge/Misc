using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class GameEnvironment
    {
        public required IFootballRepository FootballRepository { get; init; }
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; set; }
        public required IDebugContextWriter DebugContextWriter { get; init; }
        public PlayContext? CurrentPlayContext { get; set; }
        public required GameRecord CurrentGameRecord { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public required IReadOnlyList<PlayerRosterPosition> AwayActiveRoster { get; set; }
        public required IReadOnlyList<PlayerRosterPosition> HomeActiveRoster { get; set; }
    }
}
