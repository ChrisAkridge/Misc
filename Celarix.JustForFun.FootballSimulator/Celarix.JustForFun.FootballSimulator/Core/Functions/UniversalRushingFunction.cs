using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Functions
{
    internal static class UniversalRushingFunction
    {
        public static RushingResult Get(double lineOfScrimmage,
            double rushingOffenseStrength,
            double rushingDefenseStrength,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams, IRandom random)
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
                Log.Information("UniversalRushingFunction: Fumble occurred.");
                return new RushingResult(WasFumbled: true, YardsGained: null);
            }

            double yardsGained;
            if (offenseSample > defenseSample)
            {
                // Good rushing attempt, this time, we were stronger than them
                var ratio = offenseSample / defenseSample;
                var mean = physicsParams["BaseFumbleReturnMean"].Value; // not sic - name reused for all running with the ball
                var stddev = physicsParams["BaseFumbleReturnStddev"].Value * ratio;
                yardsGained = Math.Abs(random.SampleNormalDistribution(mean, stddev));
                Log.Information("UniversalRushingFunction: Successful rush for {YardsGained} yards.", yardsGained);
            }
            else
            {
                // Bad rushing attempt, we were weaker than them
                var maxYardsGained = physicsParams["MaxBadReturnDistance"].Value;
                yardsGained = random.NextDouble() * maxYardsGained;
                Log.Information("UniversalRushingFunction: Minimal rush for {YardsGained} yards.", yardsGained);
            }

            // Clamp yards gained so that the new LoS is somewhere on the field, including the endzones
            var newLineOfScrimmage = lineOfScrimmage + yardsGained;
            if (newLineOfScrimmage < Constants.HomeEndLineYard)
            {
                // Figure out how far we could go to reach the end line
                yardsGained = Constants.HomeEndLineYard - lineOfScrimmage;
            }
            else if (newLineOfScrimmage > Constants.AwayEndLineYard)
            {
                yardsGained = Constants.AwayEndLineYard - lineOfScrimmage;
            }

            return new RushingResult(WasFumbled: false, YardsGained: yardsGained);
        }
    }
}
