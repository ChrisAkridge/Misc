using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class QBSneakOutcome
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var offensiveLineStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession)
                .OffensiveLineStrength;
            var defensiveLineStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent())
                .DefensiveLineStrength;
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;
            var selfSample = parameters.Random.SampleNormalDistribution(offensiveLineStrength, standardStrengthStddev);
            var opponentSample = parameters.Random.SampleNormalDistribution(defensiveLineStrength, standardStrengthStddev);
            var sneakSuccessChance = selfSample / (selfSample + opponentSample);
            var sneakSucceeded = parameters.Random.Chance(sneakSuccessChance);

            if (sneakSucceeded)
            {
                Log.Information("QBSneakOutcome: QB sneak successful, first down.");
                return PlayerDownedFunction.Get(priorState.InvolvesOffensiveRun().InvolvesAdditionalOffensivePlayer() with
                {
                    LineOfScrimmage = priorState.LineToGain!.Value,
                    LastPlayDescriptionTemplate = "{OffAbbr} QB sneak by {OffPlayer0} successful for a first down at {LoS}!"
                }, priorState.LineOfScrimmage, priorState.LineToGain.Value - priorState.LineOfScrimmage, EndzoneBehavior.StandardGameplay,
                null);
            }

            var yardsLost = -(parameters.Random.NextDouble() * 2);
            var newLineOfScrimmage = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, yardsLost);
            priorState.AddTag("negative-play");
            return PlayerDownedFunction.Get(priorState.InvolvesOffensiveRun().InvolvesAdditionalOffensivePlayer() with
            {
                LineOfScrimmage = newLineOfScrimmage.Round()
            }, priorState.LineOfScrimmage, yardsLost.Round(), EndzoneBehavior.StandardGameplay, null);
        }
    }
}
