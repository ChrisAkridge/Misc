using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.PostPlay
{
    internal static class GameClockAdjuster
    {
        public static PlayContext Adjust(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParam,
            GameRecord currentGame)
        {
            var clockDispositionOfPossessingTeam = ClockDispositionFunction.Get(priorState, parameters, physicsParam);
            var clockStoppedAfterPlay = !priorState.ClockRunning;
            Log.Verbose("Clock: Last play by team with {Disposition} clock disposition, clock running: {ClockRunning}",
                clockDispositionOfPossessingTeam, !clockStoppedAfterPlay);

            // Step 1: Compute how long the play took
            var playTimeSecondsMean = physicsParam["PlayTimeSecondsMean"].Value;
            var playTimeSecondsStdDev = physicsParam["PlayTimeSecondsStddev"].Value;
            var playDuration = parameters.Random.SampleNormalDistribution(playTimeSecondsMean, playTimeSecondsStdDev);
            Log.Verbose("Clock: Play took {PlayDuration:F2} seconds", playDuration);

            // Step 2: Compute how long it took for the team to snap the next play, based on clock disposition
            var physicsParamName = $"TimeBetweenPlays{clockDispositionOfPossessingTeam}";
            var timeBetweenPlays = physicsParam[physicsParamName].Value;
            var totalDuration = playDuration + (clockStoppedAfterPlay ? 0 : timeBetweenPlays);
            Log.Verbose("Clock: Waited {TimeBetweenPlays:F2} seconds to snap", timeBetweenPlays);

            // Step 3: Adjust the clock by subtracting the total duration from the prior state's seconds left in period
            var newSecondsLeftInPeriod = priorState.SecondsLeftInPeriod - totalDuration.Round();

            if (newSecondsLeftInPeriod > 0)
            {
                Log.Verbose("Clock: {Period} {Clock}", priorState.PeriodNumber.ToPeriodDisplayString(), newSecondsLeftInPeriod.ToMinuteSecondString());
                return priorState with
                {
                    SecondsLeftInPeriod = newSecondsLeftInPeriod
                };
            }

            var endedPeriod = priorState.PeriodNumber;
            if (endedPeriod is 1 or 3)
            {
                Log.Verbose("Clock: End of {Period}", endedPeriod.ToPeriodDisplayString());
                return priorState with
                {
                    PeriodNumber = endedPeriod + 1,
                    SecondsLeftInPeriod = Constants.SecondsPerQuarter,
                    // Don't use WithNextState here, we don't need to keep history by this point
                    NextState = PlayEvaluationState.PlayEvaluationComplete
                };
            }
            else if (endedPeriod is 2)
            {
                Log.Verbose("Clock: End of {Period}, starting second half", endedPeriod.ToPeriodDisplayString());
                return priorState with
                {
                    PeriodNumber = 3,
                    SecondsLeftInPeriod = Constants.SecondsPerQuarter,
                    NextState = PlayEvaluationState.EndOfHalf
                };
            }
            else if (endedPeriod is 4)
            {
                if (priorState.HomeScore == priorState.AwayScore)
                {
                    Log.Verbose("Clock: End of regulation, starting overtime");
                    return priorState with
                    {
                        PeriodNumber = 5,
                        SecondsLeftInPeriod = Constants.SecondsPerOvertimePeriod,
                        NextState = PlayEvaluationState.EndOfHalf
                    };
                }
                Log.Verbose("Clock: End of regulation, game over");
                return priorState with
                {
                    NextState = PlayEvaluationState.EndOfGame
                };
            }
            else if (endedPeriod >= 5)
            {
                var isPlayoffGame = currentGame.GameType == GameType.Postseason;
                var bothTeamsHadPossession = currentGame.TeamDriveRecords
                    .Where(r => r.QuarterNumber >= 5)
                    .Select(r => r.TeamID)
                    .Distinct()
                    .Count() == 2;
                var isScoreTied = priorState.HomeScore == priorState.AwayScore;
                if (!isPlayoffGame)
                {
                    Log.Verbose("Clock: End of overtime, game over (regular season)");
                    return priorState with
                    {
                        NextState = PlayEvaluationState.EndOfGame
                    };
                }
                else if (bothTeamsHadPossession && !isScoreTied)
                {
                    Log.Verbose("Clock: End of overtime, both teams had possession, game over (postseason)");
                    return priorState with
                    {
                        NextState = PlayEvaluationState.EndOfGame
                    };
                }
                else
                {
                    Log.Verbose("Clock: End of {Period}, starting new overtime period (postseason)", endedPeriod.ToPeriodDisplayString());
                    return priorState with
                    {
                        PeriodNumber = endedPeriod + 1,
                        SecondsLeftInPeriod = Constants.SecondsPerOvertimePeriod,
                        NextState = (endedPeriod % 2 == 0)
                            ? PlayEvaluationState.EndOfHalf
                            : PlayEvaluationState.PlayEvaluationComplete
                    };
                }
            }

            throw new InvalidOperationException($"Invalid period number encountered: {endedPeriod}");
        }
    }
}
