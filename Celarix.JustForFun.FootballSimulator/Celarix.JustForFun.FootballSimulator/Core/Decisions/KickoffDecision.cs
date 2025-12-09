using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class KickoffDecision
    {
        // We find the foundational idea here:
        // - Decisions can call other decisions or outcomes, but then end result is always that
        //   they take in a GameState and return another GameState. GameStates are immutable records,
        //   so each decision or outcome produces a new GameState based on the prior one.

        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            // The kicking team is marked as having possession until one team or the other recovers the kick
            var kickingTeam = parameters.GetTeam(priorState.TeamWithPossession);
            var kickingTeamDisposition = parameters.GetDispositionForTeam(priorState.TeamWithPossession);

            if (kickingTeamDisposition == TeamDisposition.UltraConservative)
            {
                Log.Verbose("KickoffDecision: Kicking team disposition is UltraConservative; performing normal kickoff.");
                return NormalKickoffOutcome.Run(priorState, parameters, physicsParams);
            }
            else if (kickingTeamDisposition == TeamDisposition.Conservative)
            {
                var pointsPerMinuteThreshold = physicsParams["OnsideKickPointsPerMinuteThreshold"].Value;
                var minutesLeftInGame = priorState.TotalSecondsLeftInGame() / 60;
                var scoreDifference = priorState.GetScoreDifferenceForTeam(priorState.TeamWithPossession);
                if (scoreDifference < 0
                    && Math.Abs(scoreDifference) / minutesLeftInGame >= pointsPerMinuteThreshold)
                {
                    Log.Verbose("KickoffDecision: Down by a lot with little time left; performing onside kick attempt.");
                    return OnsideKickAttemptOutcome.Run(priorState, parameters, physicsParams);
                }
                else
                {
                    Log.Verbose("KickoffDecision: Performing normal kickoff.");
                    return NormalKickoffOutcome.Run(priorState, parameters, physicsParams);
                }
            }
            else
            {
                Log.Verbose("KickoffDecision: Kicking team disposition is Insane or UltraInsane; performing onside kick attempt.");
                return OnsideKickAttemptOutcome.Run(priorState, parameters, physicsParams);
            }
        }
    }
}
