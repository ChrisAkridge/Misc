using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
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
                    // Always fair catch
                    return FairCatch(priorState);
                }
                else
                {
                    // Insane or UltraInsane, always return
                    return KickOrPuntReturnOutcome.Run(priorState, parameters, physicsParams);
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
                return KickOrPuntReturnOutcome.Run(priorState, parameters, physicsParams);
            }

            var gameCloseToEndingThreshold = physicsParams["ReturnKickCloseGameTimeThreshold"].Value;
            if (gameCloseToEndingThreshold < priorState.TotalSecondsLeftInGame())
            {
                return KickOrPuntReturnOutcome.Run(priorState, parameters, physicsParams);
            }
            return FairCatch(priorState);
        }

        private static GameState FairCatch(GameState priorState)
        {
            if (priorState.InternalYardToTeamYard(priorState.LineOfScrimmage).TeamYard < 0)
            {
                // Touchback
                int lineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35);
                return priorState with
                {
                    TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                    PossessionOnPlay = priorState.TeamWithPossession.Opponent().ToPossessionOnPlay(),
                    LineOfScrimmage = lineOfScrimmage,
                    LineToGain = priorState.AddYardsForPossessingTeam(lineOfScrimmage, 10).Round()
                };
            }
            else
            {
                // Fair catch on field
                return priorState with
                {
                    TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                    PossessionOnPlay = priorState.TeamWithPossession.Opponent().ToPossessionOnPlay(),
                    LineToGain = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, 10).Round()
                };
            }
        }
    }
}
