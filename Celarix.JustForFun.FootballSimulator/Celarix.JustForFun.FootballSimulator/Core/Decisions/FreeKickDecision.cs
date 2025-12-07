using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class FreeKickDecision
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var kickingTeamSelfEstimate = parameters.GetEstimateOfTeamByTeam(
                priorState.TeamWithPossession,
                priorState.TeamWithPossession);
            var kickingTeamOtherEstimate = parameters.GetEstimateOfTeamByTeam(
                priorState.TeamWithPossession,
                priorState.TeamWithPossession.Opponent());

            if (kickingTeamSelfEstimate.KickDefenseStrength >= kickingTeamOtherEstimate.KickReturnStrength)
            {
                return NormalKickoffOutcome.Run(priorState, parameters, physicsParams);
            }
            return PuntOutcome.Run(priorState, parameters, physicsParams);
        }
    }
}
