using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class KickOrPuntReturnOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var kickingStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession)
                .KickingStrength;
            var kickDefenseStrength = parameters.GetActualStrengthsForTeam(priorState.OtherTeam(priorState.TeamWithPossession))
                .KickDefenseStrength;
            var rushAttemptResult = UniversalRushingFunction.Get(kickingStrength,
                kickDefenseStrength,
                physicsParams,
                parameters.Random);

            // WYLO: outcome VI
        }
    }
}
