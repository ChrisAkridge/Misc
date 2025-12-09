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
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var possessingTeamDisposition = parameters.GetDispositionForTeam(priorState.TeamWithPossession);
            if (possessingTeamDisposition is TeamDisposition.Insane or TeamDisposition.UltraInsane)
            {
                Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Team with possession is {Disposition}, attempting return.",
                    possessingTeamDisposition);
                return FumbleOrInterceptionReturnOutcome.Run(priorState, parameters, physicsParams);
            }
            else if (possessingTeamDisposition == TeamDisposition.UltraConservative)
            {
                Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Team with possession is UltraConservative, not attempting return.");
                return priorState.WithFirstDownLineOfScrimmage(priorState.LineOfScrimmage,
                    priorState.TeamWithPossession,
                    "{OffAbbr} fumble recovered by {DefAbbr} at {LoS}, will not return. First down.");
            }
            else if (priorState.NextPlay == NextPlayKind.ConversionAttempt)
            {
                Log.Verbose("ReturnFumbledOrInterceptedBallDecision: On conversion attempt, attempting return.");
                return FumbleOrInterceptionReturnOutcome.Run(priorState, parameters, physicsParams);
            }
            
            var secondsLeftInGame = priorState.TotalSecondsLeftInGame();
            bool isPossessingTeamLeading = priorState.GetScoreForTeam(priorState.TeamWithPossession) > priorState.GetScoreForTeam(priorState.TeamWithPossession.Opponent());
            if (parameters.GameType != GameType.Postseason && secondsLeftInGame <= 10 * 60 && isPossessingTeamLeading)
            {
                // yeah I wonder if I typoed here, if we really mean to return here
                Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Late in game with lead, attempting return.");
                return FumbleOrInterceptionReturnOutcome.Run(priorState, parameters, physicsParams);
            }

            var teamYard = priorState.InternalYardToTeamYard(priorState.LineOfScrimmage);
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
                    Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Estimated running offense strength ({OffStr}) > estimated running defense strength ({DefStr}), attempting return.",
                        estimatedRunningOffenseStrength,
                        estimatedRunningDefenseStrength);
                    return FumbleOrInterceptionReturnOutcome.Run(priorState, parameters, physicsParams);
                }

                var scoreMargin = priorState.GetScoreDifferenceForTeam(priorState.TeamWithPossession);
                var scoreMarginPointsPerMinute = scoreMargin / (secondsLeftInGame / 60d);
                if (scoreMargin <= -1 && scoreMargin >= -8 && Math.Abs(scoreMarginPointsPerMinute) >= 1d)
                {
                    Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Possessing team is trailing by 1-8 points with at least 1 point per minute pace, attempting return.");
                    return FumbleOrInterceptionReturnOutcome.Run(priorState, parameters, physicsParams);
                }
            }
            Log.Verbose("ReturnFumbledOrInterceptedBallDecision: Not attempting return.");
            return priorState.WithFirstDownLineOfScrimmage(priorState.LineOfScrimmage,
                priorState.TeamWithPossession,
                "{OffAbbr} fumble recovered by {DefAbbr} at {LoS}, will not return. First down.");
        }
    }
}
