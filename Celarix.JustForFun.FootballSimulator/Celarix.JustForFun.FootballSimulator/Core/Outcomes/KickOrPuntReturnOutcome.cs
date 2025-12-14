using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
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
            var kickDefenseStrength = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .KickDefenseStrength;
            var rushAttemptResult = UniversalRushingFunction.Get(kickingStrength,
                kickDefenseStrength,
                physicsParams,
                parameters.Random);

            if (rushAttemptResult.WasFumbled)
            {
                Log.Verbose("Fumble on kick/punt return!");
                return priorState.WithNextState(GameplayNextState.FumbledLiveBallOutcome);
            }

            var yardsGained = rushAttemptResult.YardsGained ?? throw new InvalidOperationException("Rushing function specified no yards gained value, but ball was not fumbled; must have value.");
            GameState newState = priorState with
            {
                TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                LastPlayDescriptionTemplate = "{OffTeam} {OffPlayer0} returned ball to the {LoS}.",
                ClockRunning = true
            };
            var newLineOfScrimmage = newState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, yardsGained);
            return PlayerDownedFunction.Get(newState, parameters, physicsParams, priorState.LineOfScrimmage, yardsGained.Round(), EndzoneBehavior.StandardGameplay, null);
        }
    }
}
