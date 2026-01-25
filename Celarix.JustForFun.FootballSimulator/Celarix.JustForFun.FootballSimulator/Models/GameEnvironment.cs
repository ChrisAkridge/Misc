using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class GameEnvironment
    {
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; set; }
        public PlayContext? CurrentPlayContext { get; set; }
        public required GameRecord CurrentGameRecord { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
    }
}
