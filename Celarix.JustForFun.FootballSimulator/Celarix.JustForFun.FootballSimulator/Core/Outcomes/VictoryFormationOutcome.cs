using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class VictoryFormationOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            Log.Information("VictoryFormationOutcome: Taking victory formation.");
            return PlayerDownedFunction.Get(priorState,
                parameters,
                physicsParams,
                priorState.LineOfScrimmage,
                -2,
                EndzoneBehavior.StandardGameplay,
                null);
        }
    }
}
