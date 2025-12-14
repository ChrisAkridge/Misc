using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class SignalFairCatchDecision
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            // Prerequisites:
            // - priorState.TeamWithPossession here is always the team receiving the kick
            // - priorState.LineOfScrimmage is where the ball is caught

            var receivingTeamDisposition = parameters.GetDispositionForTeam(priorState.TeamWithPossession);
            if (receivingTeamDisposition != TeamDisposition.Conservative)
            {
                if (receivingTeamDisposition == TeamDisposition.UltraConservative)
                {
                    Log.Verbose("SignalFairCatchDecision: Team disposition is UltraConservative, always fair catch.");
                    return FairCatch(priorState);
                }
                else
                {
                    Log.Verbose("SignalFairCatchDecision: Team disposition is Insane or UltraInsane, always return.");
                    return priorState.WithNextState(GameplayNextState.KickOrPuntReturnOutcome);
                }
            }

            var receivingTeamEstimateOfOwnStrengths = parameters.GetEstimateOfTeamByTeam(priorState.TeamWithPossession,
                priorState.TeamWithPossession);
            var receivingTeamEstimateOfOtherStrengths = parameters.GetEstimateOfTeamByTeam(
                priorState.TeamWithPossession,
                priorState.TeamWithPossession.Opponent());
            var receivingKickReturnStrengthEstimate = receivingTeamEstimateOfOwnStrengths.KickReturnStrength;
            var kickingKickDefenseStrengthEstimate = receivingTeamEstimateOfOtherStrengths.KickDefenseStrength;
            var strengthRatio = receivingKickReturnStrengthEstimate / kickingKickDefenseStrengthEstimate;
            var threshold = physicsParams["KickReturnChoiceThreshold"].Value;
            if (strengthRatio > threshold)
            {
                Log.Verbose("SignalFairCatchDecision: Strength ratio {StrengthRatio:F2} exceeds threshold {Threshold:F2}, returning kick.",
                    strengthRatio,
                    threshold);
                return priorState.WithNextState(GameplayNextState.KickOrPuntReturnOutcome);
            }

            var gameCloseToEndingThreshold = physicsParams["ReturnKickCloseGameTimeThreshold"].Value;
            if (gameCloseToEndingThreshold < priorState.TotalSecondsLeftInGame())
            {
                Log.Verbose("SignalFairCatchDecision: Game not close to ending (time left {TimeLeft:F2}s exceeds threshold {Threshold:F2}s), returning kick.",
                    priorState.TotalSecondsLeftInGame(),
                    gameCloseToEndingThreshold);
                return priorState.WithNextState(GameplayNextState.KickOrPuntReturnOutcome);
            }
            return FairCatch(priorState);
        }

        private static GameState FairCatch(GameState priorState)
        {
            if (priorState.InternalYardToTeamYard(priorState.LineOfScrimmage).TeamYard < 0)
            {
                // Touchback
                int lineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35);
                return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
                {
                    TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                    PossessionOnPlay = priorState.TeamWithPossession.Opponent().ToPossessionOnPlay(),
                    LineOfScrimmage = lineOfScrimmage,
                    LineToGain = priorState.AddYardsForPossessingTeam(lineOfScrimmage, 10).Round(),
                    LastPlayDescriptionTemplate = "{DefAbbr} touchback, ball placed at {LoS}."
                };
            }
            else
            {
                // Fair catch on field
                return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
                {
                    TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                    PossessionOnPlay = priorState.TeamWithPossession.Opponent().ToPossessionOnPlay(),
                    LineToGain = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, 10).Round(),
                    LastPlayDescriptionTemplate = "{DefAbbr} {DefPlayer0} signals for fair catch at {LoS}."
                };
            }
        }
    }
}
