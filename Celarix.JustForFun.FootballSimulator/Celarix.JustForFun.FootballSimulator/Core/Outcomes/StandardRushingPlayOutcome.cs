using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class StandardRushingPlayOutcome
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var selfRushingStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession)
                .RunningOffenseStrength;
            var opposingRushingDefenseStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .RunningDefenseStrength;
            var rushResult = UniversalRushingFunction.Get(priorState.LineOfScrimmage, selfRushingStrength,
                opposingRushingDefenseStrength,
                physicsParams,
                parameters.Random);

            if (rushResult.WasFumbled)
            {
                Log.Information("StandardRushingPlayOutcome: Fumble occurred during rushing play.");
                return priorState
                    .InvolvesOffensiveRun()
                    .WithNextState(PlayEvaluationState.FumbledLiveBallOutcome);
            }

            return PlayerDownedFunction.Get(priorState.InvolvesOffensiveRun().InvolvesAdditionalOffensivePlayer(),
                priorState.LineOfScrimmage,
                (int)rushResult.YardsGained!.Value,
                EndzoneBehavior.StandardGameplay,
                null);
        }
    }
}
