using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class GameEnvironment
    {
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; set; }
        public PlayContext? CurrentPlayContext { get; set; }
    }
}
