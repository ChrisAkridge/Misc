using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using MathNet.Numerics.Distributions;

namespace Celarix.JustForFun.FootballSimulator
{
    internal static class Helpers
    {
        private static readonly Dictionary<(double mean, double standardDeviation), Normal> distributionCache =
            new Dictionary<(double mean, double standardDeviation), Normal>();

        public static GameTeam OtherTeam(GameTeam team) =>
            team switch
            {
                GameTeam.Home => GameTeam.Away,
                GameTeam.Away => GameTeam.Home,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static DriveDirection TowardOpponentEndzone(GameTeam team) =>
            team switch
            {
                GameTeam.Home => DriveDirection.TowardAwayEndzone,
                GameTeam.Away => DriveDirection.TowardHomeEndzone,
                _ => throw new ArgumentOutOfRangeException()
            };

        public static int? YardsDownfield(int lineOfScrimmage, int distance, DriveDirection direction)
        {
            var firstDownLine = lineOfScrimmage
                + (direction == DriveDirection.TowardHomeEndzone
                    ? -distance
                    : distance);

            return firstDownLine is < 0 or > 100
                ? // It's goal-to-go!
                null
                : firstDownLine;
        }

        public static int TeamYardLineToInternalYardLine(int teamYardLine, GameTeam team) =>
            team == GameTeam.Home
                ? teamYardLine
                : 100 - teamYardLine;

        public static bool IsYardLineBeyondYardLine(int yardLineA, int yardLineB, DriveDirection direction) =>
            direction == DriveDirection.TowardHomeEndzone
                ? yardLineA < yardLineB
                : yardLineA > yardLineB;

        public static double StandardAsymptoticFunction(double x, double growthDecelerator) => x / (x + growthDecelerator);

        public static double SampleNormalDistribution(double mean, double standardDeviation, Random random)
        {
            if (!distributionCache.ContainsKey((mean, standardDeviation)))
            {
                distributionCache.Add((mean, standardDeviation), new Normal(mean, standardDeviation, random));
            }
            
            var normalDistribution = distributionCache[(mean, standardDeviation)];

            return normalDistribution.Sample();
        }

        public static string FormatSeconds(int seconds) => $"{seconds / 60}:{seconds % 60:D2}";
    }
}
