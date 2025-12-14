using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.PostPlay
{
    internal static class GameClockAdjuster
    {
        public static GameState Adjust(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParam)
        {
            var clockDispositionOfPossessingTeam = ClockDispositionFunction.Get(priorState, parameters, physicsParam);
            var clockStoppedAfterPlay = !priorState.ClockRunning;

            // Step 1: Compute how long the play took
            var playTimeSecondsMean = physicsParam["PlayTimeSecondsMean"].Value;
            var playTimeSecondsStdDev = physicsParam["PlayTimeSecondsStdDev"].Value;
            var playDuration = parameters.Random.SampleNormalDistribution(playTimeSecondsMean, playTimeSecondsStdDev);

            // Step 2: Compute how long it took for the team to snap the next play, based on clock disposition
            var physicsParamName = $"TimeBetweenPlays{clockDispositionOfPossessingTeam}";
            var timeBetweenPlays = physicsParam[physicsParamName].Value;
            var totalDuration = playDuration + (clockStoppedAfterPlay ? 0 : timeBetweenPlays);

            // Step 3: Adjust the clock by subtracting the total duration from the prior state's seconds left in period
            var newSecondsLeftInPeriod = priorState.SecondsLeftInPeriod - totalDuration.Round();

            if (newSecondsLeftInPeriod > 0)
            {
                return priorState with
                {
                    SecondsLeftInPeriod = newSecondsLeftInPeriod
                };
            }

            int nextPeriodNumber = priorState.PeriodNumber + 1;
            return priorState.WithNextState(priorState.PeriodNumber % 2 == 0
                    ? GameplayNextState.EndOfHalf
                    : GameplayNextState.PlayEvaluationComplete) with
            {
                PeriodNumber = nextPeriodNumber,
                SecondsLeftInPeriod = (nextPeriodNumber <= 4)
                    ? Constants.SecondsPerQuarter
                    : Constants.SecondsPerOvertimePeriod
            };
        }
    }
}
