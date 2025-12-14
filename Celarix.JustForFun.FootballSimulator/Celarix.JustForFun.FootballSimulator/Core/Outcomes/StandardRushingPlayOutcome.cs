using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class StandardRushingPlayOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var selfRushingStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession)
                .RunningOffenseStrength;
            var opposingRushingDefenseStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .RunningDefenseStrength;
            var rushResult = UniversalRushingFunction.Get(selfRushingStrength,
                opposingRushingDefenseStrength,
                physicsParams,
                parameters.Random);

            if (rushResult.WasFumbled)
            {
                return priorState.WithNextState(GameplayNextState.FumbledLiveBallOutcome);
            }

            return PlayerDownedFunction.Get(priorState,
                parameters,
                physicsParams,
                priorState.LineOfScrimmage,
                (int)rushResult.YardsGained!.Value,
                EndzoneBehavior.StandardGameplay,
                null);
        }
    }
}
