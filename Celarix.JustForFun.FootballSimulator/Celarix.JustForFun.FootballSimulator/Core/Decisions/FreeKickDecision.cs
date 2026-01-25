using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class FreeKickDecision
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;

            var kickingTeamSelfEstimate = parameters.GetEstimateOfTeamByTeam(
                priorState.TeamWithPossession,
                priorState.TeamWithPossession);
            var kickingTeamOtherEstimate = parameters.GetEstimateOfTeamByTeam(
                priorState.TeamWithPossession,
                priorState.TeamWithPossession.Opponent());

            if (kickingTeamSelfEstimate.KickDefenseStrength >= kickingTeamOtherEstimate.KickReturnStrength)
            {
                Log.Information("FreeKickDecision: We're better at kick defense, choosing normal kickoff outcome.");
                return priorState.WithNextState(PlayEvaluationState.NormalKickoffOutcome);
            }
            Log.Information("FreeKickDecision: They're better at kick returns, choosing punt outcome.");
            return priorState.WithNextState(PlayEvaluationState.PuntOutcome);
        }
    }
}
