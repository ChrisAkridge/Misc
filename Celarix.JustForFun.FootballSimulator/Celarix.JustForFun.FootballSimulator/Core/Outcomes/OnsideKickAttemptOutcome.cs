using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class OnsideKickAttemptOutcome
    {
        public static PlayContext Run(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var selfStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var opponentStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent());
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;

            var selfSample = parameters.Random.SampleNormalDistribution(selfStrengths.KickingStrength, standardStrengthStddev);
            var opponentSample = parameters.Random.SampleNormalDistribution(opponentStrengths.DefensiveLineStrength, standardStrengthStddev);
            var percentagePointDelta = 1 - (opponentSample / selfSample);
            var baseRecoveryChance = physicsParams["BaseOnsideRecoveryChance"].Value;
            double recoveryChance;
            if (percentagePointDelta < 0)
            {
                var invertedDelta = Math.Abs(percentagePointDelta);
                var alpha = invertedDelta * 100;
                var beta = Math.Pow(2, alpha);
                
                recoveryChance = baseRecoveryChance / beta;
            }
            else if (percentagePointDelta > 0)
            {
                var alpha = percentagePointDelta * physicsParams["OnsidePositiveRecoveryMultiplier"].Value;
                var beta = alpha + baseRecoveryChance;

                recoveryChance = Math.Clamp(beta, baseRecoveryChance, 0.99);
            }
            else
            {
                recoveryChance = baseRecoveryChance;
            }

            var kickRecovered = parameters.Random.Chance(recoveryChance);
            var recoveryDistance = parameters.Random.SampleNormalDistribution(
                    physicsParams["OnsideRecoveryDistanceMean"].Value,
                    physicsParams["OnsideRecoveryDistanceStddev"].Value);
            var newLineOfScrimmage = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, recoveryDistance.Round());
            if (kickRecovered)
            {
                Log.Information("OnsideKickAttemptOutcome: Kicking team recovers own onside kick.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, priorState.TeamWithPossession,
                    "{OffAbbr} recovers the onside kick at {LoS}!", clockRunning: false, startOfDrive: true);
            }
            Log.Information("OnsideKickAttemptOutcome: Receiving team recovers onside kick.");
            return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, priorState.TeamWithPossession.Opponent(),
                "{OffAbbr} recovers the onside kick of {DefAbbr} at {LoS}!", clockRunning: false, startOfDrive: true);
        }
    }
}
