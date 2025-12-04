using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using MathNet.Numerics.Distributions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator
{
    public static class Helpers
    {
	    public static int SchedulingRandomSeed => -1039958483;

		private static readonly Dictionary<(double mean, double standardDeviation), Normal> distributionCache =
            new Dictionary<(double mean, double standardDeviation), Normal>();

        public static Dictionary<BasicTeamInfo, int> GetDefaultPreviousSeasonDivisionRankings(IReadOnlyList<BasicTeamInfo> teams)
        {
            Log.Information("No previous season division rankings available, generating default...");

            var random = new Random(SchedulingRandomSeed);
            var rankings = new Dictionary<BasicTeamInfo, int>();

            var conferences = new[]
            {
                Conference.AFC, Conference.NFC
            };

            var divisions = new[]
            {
                Division.East, Division.North, Division.South, Division.West, Division.Extra
            };

            foreach (var divisionTeams in conferences.SelectMany(c => divisions.Select(d => GetTeamsInDivision(teams, c, d).ToList())))
            {
                divisionTeams.Shuffle(random);

                for (int i = 0; i < 4; i++) { rankings[divisionTeams[i]] = i + 1; }
            }

            return rankings;
        }

        public static IEnumerable<BasicTeamInfo> GetTeamsInDivision(IEnumerable<BasicTeamInfo> teams, Conference conference, Division division) =>
			teams.Where(t => t.Conference == conference && t.Division == division);

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
                    ? distance
                    : -distance);

            return firstDownLine is < 0 or > 100
                ? // It's goal-to-go!
                null
                : firstDownLine;
        }

        public static int TeamYardLineToInternalYardLine(int teamYardLine, GameTeam team) =>
            team == GameTeam.Home
                ? 100 - teamYardLine
                : teamYardLine;

        public static bool IsYardLineBeyondYardLine(int yardLineA, int yardLineB, DriveDirection direction) =>
            direction == DriveDirection.TowardHomeEndzone
                ? yardLineA > yardLineB
                : yardLineA < yardLineB;

        public static double StandardAsymptoticFunction(double x, double growthDecelerator) => x / (x + growthDecelerator);

        public static double SampleNormalDistribution(double mean, double standardDeviation, Random random)
        {
            standardDeviation = Math.Abs(standardDeviation);
            
            if (!distributionCache.ContainsKey((mean, standardDeviation)))
            {
                distributionCache.Add((mean, standardDeviation), new Normal(mean, standardDeviation, random));
            }
            
            var normalDistribution = distributionCache[(mean, standardDeviation)];

            return normalDistribution.Sample();
        }

        public static double SampleNormalDistribution(NormalDistributionParameters parameters, double value, Random random) =>
            value < 0d
                ? SampleNormalDistribution(parameters.MeanAtZero - (parameters.MeanReductionPerUnitValue * Math.Abs(value)),
                    parameters.StandardDeviationAtZero, random)
                : SampleNormalDistribution(parameters.MeanAtZero,
                    parameters.StandardDeviationAtZero + (parameters.StandardDeviationIncreasePerUnitValue * value),
                    random);

        public static string FormatSeconds(int seconds) => $"{seconds / 60}:{seconds % 60:D2}";

        public static string DetermineArticle(string subject) =>
            new[]
            {
                'a', 'e', 'i', 'o', 'u'
            }.Contains(subject.ToLowerInvariant()[0])
                ? "an"
                : "a";
    }
}
