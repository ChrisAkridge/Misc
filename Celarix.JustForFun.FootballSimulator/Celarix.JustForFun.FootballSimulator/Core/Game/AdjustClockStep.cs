using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Game
{
    internal static class AdjustClockStep
    {
        public static GameContext Run(GameContext context)
        {
            var playContext = context.Environment.CurrentPlayContext!;
            var physicsParam = context.Environment.PhysicsParams;
            var random = context.Environment.RandomFactory.Create();

            var clockDispositionOfPossessingTeam = ClockDispositionFunction.Get(playContext);
            var clockStoppedAfterPlay = !playContext.ClockRunning;
            Log.Verbose("Clock: Last play by team with {Disposition} clock disposition, clock running: {ClockRunning}",
                clockDispositionOfPossessingTeam, !clockStoppedAfterPlay);

            // Step 1: Compute how long the play took
            var playTimeSecondsMean = physicsParam["PlayTimeSecondsMean"].Value;
            var playTimeSecondsStdDev = physicsParam["PlayTimeSecondsStddev"].Value;
            var playDuration = random.SampleNormalDistribution(playTimeSecondsMean, playTimeSecondsStdDev);
            Log.Verbose("Clock: Play took {PlayDuration:F2} seconds", playDuration);

            // Step 2: Compute how long it took for the team to snap the next play, based on clock disposition
            var physicsParamName = $"TimeBetweenPlays{clockDispositionOfPossessingTeam}";
            var timeBetweenPlays = physicsParam[physicsParamName].Value;
            var totalDuration = playDuration + (clockStoppedAfterPlay ? 0 : timeBetweenPlays);
            Log.Verbose("Clock: Waited {TimeBetweenPlays:F2} seconds to snap", timeBetweenPlays);

            // Step 3: Adjust the clock by subtracting the total duration from the prior state's seconds left in period
            var newSecondsLeftInPeriod = Math.Clamp(playContext.SecondsLeftInPeriod - totalDuration.Round(),
                0, Constants.SecondsPerQuarter);

            Log.Verbose("Clock: {Period} {Clock}", playContext.PeriodNumber.ToPeriodDisplayString(), newSecondsLeftInPeriod.ToMinuteSecondString());
            context.Environment.CurrentPlayContext = playContext with
            {
                SecondsLeftInPeriod = newSecondsLeftInPeriod
            };

            return context.WithNextState(GameState.PostPlayCheck);
        }
    }
}
