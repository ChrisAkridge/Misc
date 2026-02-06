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
        public static PlayContext Run(PlayContext priorState)
        {
            priorState = priorState.InvolvesOffensivePass()
                .InvolvesAdditionalOffensivePlayer();

            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var possessingTeam = priorState.TeamWithPossession;
            var selfStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var opponentStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent());
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;
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
            var throwingYardLocation = object.InternalYardToTeamYard(priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, -5).Round());
            if (throwingYardLocation.Team == possessingTeam && throwingYardLocation.TeamYard <= -10)
            {
                Log.Information("PlayerDownedFunction: Safety on Hail Mary from own endzone!");
                priorState.AddTag("safety-scored");
                return priorState.WithScoreChange(possessingTeam.Opponent(), 2)
                    .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.FreeKick,
                    LineOfScrimmage = object.TeamYardToInternalYard(possessingTeam.Opponent(), 25),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.Opponent().ToPossessionOnPlay(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{DefAbbr} safety! {OffPlayer0} dropped back out of his own endzone.",
                    DriveResult = DriveResult.Safety
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
                Log.Information("PlayerDownedFunction: Interception on Hail Mary!");
                priorState.AddTag("interception");
                return priorState.WithAdditionalParameter<bool?>("WasIntercepted", true)
                    .WithNextState(PlayEvaluationState.FumbledLiveBallOutcome)
                    .InvolvesAdditionalDefensivePlayer()
                with
                {
                    ClockRunning = false,
                    LineOfScrimmage = object.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 0),
                    DriveResult = DriveResult.Interception
                };
            }
            else if (someoneCaughtIt)
            {
                Log.Information("PlayerDownedFunction: Touchdown on Hail Mary!");
                priorState.AddTag("touchdown-scored");
                priorState.AddTag("hail-mary-success");
                return priorState.WithScoreChange(possessingTeam, 6)
                    .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                    .InvolvesAdditionalOffensivePlayer()
                with
                {
                    NextPlay = NextPlayKind.ConversionAttempt,
                    LineOfScrimmage = object.TeamYardToInternalYard(possessingTeam.Opponent(), 15),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} touchdown! Scored by {OffPlayer0}."
                };
            }

            // Get it as far as we can.
            return priorState.WithNextState(PlayEvaluationState.StandardLongPassingPlayOutcome);
        }
    }
}
