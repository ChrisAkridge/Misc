using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class PlayEnvironment
    {
        public required GameDecisionParameters DecisionParameters { get; init; }
        public required IReadOnlyDictionary<string, PhysicsParam> PhysicsParams { get; init; }
        public required IEventBus EventBus { get; init; }
    }
}
