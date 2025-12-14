using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class HailMaryOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var possessingTeam = priorState.TeamWithPossession;
            var selfStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var opponentStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent());
            var standardStrengthStddev = physicsParams["StandardStrengthStdDev"].Value;
            var selfSample = parameters.Random.SampleNormalDistribution(selfStrengths.PassingOffenseStrength, standardStrengthStddev);
            var opponentSample = parameters.Random.SampleNormalDistribution(opponentStrengths.PassingDefenseStrength, standardStrengthStddev);
            var ratio = selfSample / opponentSample;
            var targetEndzoneYard = priorState.TeamWithPossession == GameTeam.Away ? 0 : 100;
            var yardsToEndzone = priorState.DistanceForPossessingTeam(priorState.LineOfScrimmage, targetEndzoneYard);
            // Take 5 yards off for the quarterback's dropback
            yardsToEndzone = priorState.AddYardsForPossessingTeam(yardsToEndzone, -5);

            // Check to see if we dropped back out of the back of our own endzone
            // This really shouldn't be possible in real games or even this simulation
            // but I find it funny, so I'll leave it in. Most likely to happen with
            // the decision loop choosing a fake field goal (-15 yards) and then choosing
            // a Hail Mary on the next play.
            var throwingYardLocation = priorState.InternalYardToTeamYard(priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, -5).Round());
            if (throwingYardLocation.Team == possessingTeam && throwingYardLocation.TeamYard <= -10)
            {
                Log.Verbose("PlayerDownedFunction: Safety on Hail Mary from own endzone!");
                return priorState.WithScoreChange(possessingTeam.Opponent(), 2)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.FreeKick,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam.Opponent(), 25),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.Opponent().ToPossessionOnPlay(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{DefAbbr} safety! {OffPlayer0} dropped back out of his own endzone."
                };
            }

            var anyoneCatchesChance = ratio;
            if (yardsToEndzone > 50)
            {
                var chanceMultiplierPerYard = physicsParams["HailMaryChanceMultiplierPerYard"].Value;
                var multiplier = Math.Pow(chanceMultiplierPerYard, yardsToEndzone - 50);
                anyoneCatchesChance *= multiplier;
            }
            anyoneCatchesChance = Math.Clamp(anyoneCatchesChance, 0, 1);

            var someoneCaughtIt = parameters.Random.Chance(anyoneCatchesChance);
            var interceptionChance = physicsParams["HailMaryInterceptionChance"].Value;
            var wasIntercepted = someoneCaughtIt && parameters.Random.Chance(interceptionChance);
            if (wasIntercepted)
            {
                return priorState.WithAdditionalParameter<bool?>("WasIntercepted", true)
                    .WithNextState(GameplayNextState.FumbledLiveBallOutcome)
                with
                {
                    ClockRunning = false,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 0)
                };
            }
            else if (someoneCaughtIt)
            {
                Log.Verbose("PlayerDownedFunction: Touchdown on Hail Mary!");
                return priorState.WithScoreChange(possessingTeam, 6)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.ConversionAttempt,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam.Opponent(), 15),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} touchdown! Scored by {OffPlayer0}."
                };
            }

            // Get it as far as we can.
            return priorState.WithNextState(GameplayNextState.StandardLongPassingPlayOutcome);
        }
    }
}
