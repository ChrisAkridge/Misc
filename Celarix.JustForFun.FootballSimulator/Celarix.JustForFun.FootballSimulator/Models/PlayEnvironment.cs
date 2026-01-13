using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class PlayEnvironment
    {
        public required GameDecisionParameters DecisionParameters { get; init; }
        public required IDictionary<string, PhysicsParam> PhysicsParams { get; init; }
    }
}
