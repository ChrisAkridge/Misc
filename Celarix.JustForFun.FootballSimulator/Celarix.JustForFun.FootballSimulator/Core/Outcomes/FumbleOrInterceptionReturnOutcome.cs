using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class FumbleOrInterceptionReturnOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var kickingStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession)
               .KickingStrength;
            var kickDefenseStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .KickDefenseStrength;
            var rushAttemptResult = UniversalRushingFunction.Get(kickingStrength,
                kickDefenseStrength,
                physicsParams,
                parameters.Random);

            if (rushAttemptResult.WasFumbled)
            {
                Log.Verbose("Fumble on fumble/interception return!");
                return FumbledLiveBallOutcome.Run(priorState,
                    parameters,
                    physicsParams);
            }

            var yardsGained = rushAttemptResult.YardsGained ?? throw new InvalidOperationException("Rushing function specified no yards gained value, but ball was not fumbled; must have value.");
            GameState newState = priorState with
            {
                TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                LastPlayDescriptionTemplate = "{OffTeam} {OffPlayer0} returned ball to the {LoS}.",
            };
            var newLineOfScrimmage = newState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, yardsGained);
            return PlayerDownedFunction.Get(newState, parameters, physicsParams, priorState.LineOfScrimmage, yardsGained.Round(), EndzoneBehavior.FumbleOrInterceptionReturn, null);
        }
    }
}
