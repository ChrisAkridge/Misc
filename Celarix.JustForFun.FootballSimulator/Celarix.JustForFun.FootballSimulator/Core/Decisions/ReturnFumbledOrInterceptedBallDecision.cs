using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class ReturnFumbledOrInterceptedBallDecision
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;

            var possessingTeamDisposition = parameters.GetDispositionForTeam(priorState.TeamWithPossession);
            if (possessingTeamDisposition is TeamDisposition.Insane or TeamDisposition.UltraInsane)
            {
                Log.Information("ReturnFumbledOrInterceptedBallDecision: Team with possession is {Disposition}, attempting return.",
                    possessingTeamDisposition);
                return priorState.WithNextState(PlayEvaluationState.FumbleOrInterceptionReturnOutcome);
            }
            else if (possessingTeamDisposition == TeamDisposition.UltraConservative)
            {
                Log.Information("ReturnFumbledOrInterceptedBallDecision: Team with possession is UltraConservative, not attempting return.");
                return priorState.WithFirstDownLineOfScrimmage(priorState.LineOfScrimmage,
                    priorState.TeamWithPossession,
                    "{OffAbbr} fumble recovered by {DefAbbr} at {LoS}, will not return. First down.",
                    startOfDrive: true)
                    .InvolvesAdditionalDefensivePlayer();
            }
            else if (priorState.NextPlay == NextPlayKind.ConversionAttempt)
            {
                Log.Information("ReturnFumbledOrInterceptedBallDecision: On conversion attempt, attempting return.");
                return priorState.WithNextState(PlayEvaluationState.FumbleOrInterceptionReturnOutcome);
            }
            
            var secondsLeftInGame = priorState.TotalSecondsLeftInGame();
            bool isPossessingTeamLeading = priorState.GetScoreForTeam(priorState.TeamWithPossession) > priorState.GetScoreForTeam(priorState.TeamWithPossession.Opponent());
            if (parameters.GameType != GameType.Postseason && secondsLeftInGame <= 10 * 60 && isPossessingTeamLeading)
            {
                // yeah I wonder if I typoed here, if we really mean to return here
                Log.Information("ReturnFumbledOrInterceptedBallDecision: Late in game with lead, attempting return.");
                return priorState.WithNextState(PlayEvaluationState.FumbleOrInterceptionReturnOutcome);
            }

            var teamYard = object.InternalYardToTeamYard(priorState.LineOfScrimmage);
            if (teamYard.Team == priorState.TeamWithPossession.Opponent()
                || teamYard.TeamYard > 5)
            {
                var possessingTeamEstimatesOfSelf = parameters.GetEstimateOfTeamByTeam(priorState.TeamWithPossession,
                    priorState.TeamWithPossession);
                var possessingTeamEstimatesOfOpponent = parameters.GetEstimateOfTeamByTeam(priorState.TeamWithPossession,
                    priorState.TeamWithPossession.Opponent());
                var estimatedRunningOffenseStrength = possessingTeamEstimatesOfSelf.RunningOffenseStrength;
                var estimatedRunningDefenseStrength = possessingTeamEstimatesOfOpponent.RunningDefenseStrength;
                if (estimatedRunningOffenseStrength > estimatedRunningDefenseStrength)
                {
                    Log.Information("ReturnFumbledOrInterceptedBallDecision: Estimated running offense strength > estimated running defense strength, attempting return.",
                        estimatedRunningOffenseStrength,
                        estimatedRunningDefenseStrength);
                    return priorState.WithNextState(PlayEvaluationState.FumbleOrInterceptionReturnOutcome);
                }

                var scoreMargin = priorState.GetScoreDifferenceForTeam(priorState.TeamWithPossession);
                var scoreMarginPointsPerMinute = scoreMargin / (secondsLeftInGame / 60d);
                if (scoreMargin <= -1 && scoreMargin >= -8 && Math.Abs(scoreMarginPointsPerMinute) >= 1d)
                {
                    Log.Information("ReturnFumbledOrInterceptedBallDecision: Possessing team is trailing by 1-8 points with at least 1 point per minute pace, attempting return.");
                    return priorState.WithNextState(PlayEvaluationState.FumbleOrInterceptionReturnOutcome);
                }
            }
            Log.Information("ReturnFumbledOrInterceptedBallDecision: Not attempting return.");
            return priorState.WithFirstDownLineOfScrimmage(priorState.LineOfScrimmage,
                priorState.TeamWithPossession,
                "{OffAbbr} fumble recovered by {DefAbbr} at {LoS}, will not return. First down.",
                startOfDrive: true)
                .InvolvesAdditionalDefensivePlayer();
        }
    }
}
