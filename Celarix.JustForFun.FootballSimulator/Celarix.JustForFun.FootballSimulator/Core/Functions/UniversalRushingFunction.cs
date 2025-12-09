using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Functions
{
    internal static class UniversalRushingFunction
    {
        public static RushingResult Get(double rushingOffenseStrength,
            double rushingDefenseStrength,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            Random random)
        {
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;
            var offenseSample = random.SampleNormalDistribution(rushingOffenseStrength, standardStrengthStddev);
            var defenseSample = random.SampleNormalDistribution(rushingDefenseStrength, standardStrengthStddev);

            // Fumble probability calculations
            var baseFumbleProbaility = physicsParams["BaseRushingFumbleChance"].Value;
            var alpha = rushingOffenseStrength / 100;
            var beta = Math.Log2(alpha);
            var gamma = -beta;
            var delta = Math.Pow(2, gamma);
            baseFumbleProbaility *= delta;
            baseFumbleProbaility = Math.Clamp(baseFumbleProbaility, 0d, 0.99d);

            if (random.Chance(baseFumbleProbaility))
            {
                Log.Verbose("UniversalRushingFunction: Fumble occurred.");
                return new RushingResult(WasFumbled: true, YardsGained: null);
            }

            if (offenseSample > defenseSample)
            {
                // Good rushing attempt, this time, we were stronger than them
                var ratio = offenseSample / defenseSample;
                var mean = physicsParams["BaseFumbleReturnMean"].Value; // not sic - name reused for all running with the ball
                var stddev = physicsParams["BaseFumbleReturnStddev"].Value * ratio;
                var yardsGained = random.SampleNormalDistribution(mean, stddev);
                Log.Verbose("UniversalRushingFunction: Successful rush for {YardsGained} yards.", yardsGained);
                return new RushingResult(WasFumbled: false, YardsGained: yardsGained);
            }
            else
            {
                // Bad rushing attempt, we were weaker than them
                var maxYardsGained = physicsParams["MaxBadReturnDistance"].Value;
                var yardsGained = random.NextDouble() * maxYardsGained;
                Log.Verbose("UniversalRushingFunction: Minimal rush for {YardsGained} yards.", yardsGained);
                return new RushingResult(WasFumbled: false, YardsGained: yardsGained);
            }
        }
    }
}
