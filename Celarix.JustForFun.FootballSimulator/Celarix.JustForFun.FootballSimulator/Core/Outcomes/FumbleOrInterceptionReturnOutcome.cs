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
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var kickingStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession)
               .KickingStrength;
            var kickDefenseStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .KickDefenseStrength;
            var rushAttemptResult = UniversalRushingFunction.Get(priorState.LineOfScrimmage, priorState.TeamWithPossession, kickingStrength,
                kickDefenseStrength,
                physicsParams,
                parameters.Random);

            if (rushAttemptResult.WasFumbled)
            {
                Log.Information("Fumble on fumble/interception return!");
                priorState.AddTag("fumbled-live-ball");
                return priorState.WithNextState(PlayEvaluationState.FumbledLiveBallOutcome);
            }

            var yardsGained = rushAttemptResult.YardsGained ?? throw new InvalidOperationException("Rushing function specified no yards gained value, but ball was not fumbled; must have value.");
            PlayContext newState = priorState.InvolvesDefenseRun()
                .InvolvesAdditionalDefensivePlayer() with
            {
                TeamWithPossession = priorState.TeamWithPossession,
                LastPlayDescriptionTemplate = "{OffTeam} {OffPlayer0} returned ball to the {LoS}.",
            };
            var newLineOfScrimmage = newState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, yardsGained);
            var endzoneBehavior = priorState.HasAdditionalParameter("IsConversionAttempt")
                ? EndzoneBehavior.ConversionAttempt
                : EndzoneBehavior.FumbleOrInterceptionReturn;
            return PlayerDownedFunction.Get(newState, priorState.LineOfScrimmage, yardsGained.Round(), endzoneBehavior, null);
        }
    }
}
