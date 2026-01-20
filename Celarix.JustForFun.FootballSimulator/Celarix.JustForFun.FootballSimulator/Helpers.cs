using Celarix.JustForFun.FootballSimulator.Core;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using MathNet.Numerics.Distributions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            var random = new System.Random(SchedulingRandomSeed);
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

        public static Dictionary<BasicTeamInfo, TeamWinLossTie> GetWinLossTies(IReadOnlyList<BasicTeamInfo> teams,
            IEnumerable<GameRecord> gameRecords)
        {
            var games = gameRecords
                .Where(g => g.GameComplete)
                .Select(g => new
                {
                    HomeTeam = new BasicTeamInfo(g.HomeTeam.TeamName, g.HomeTeam.Conference, g.HomeTeam.Division),
                    AwayTeam = new BasicTeamInfo(g.AwayTeam.TeamName, g.AwayTeam.Conference, g.AwayTeam.Division),
                    HomeScore = g.GetScoreForTeam(GameTeam.Home),
                    AwayScore = g.GetScoreForTeam(GameTeam.Away)
                })
                .ToList();
            var winLossTies = teams.ToDictionary(t => t, t => new TeamWinLossTie());
            foreach (var game in games)
            {
                if (game.HomeScore > game.AwayScore)
                {
                    winLossTies[game.HomeTeam].Wins++;
                    winLossTies[game.AwayTeam].Losses++;
                }
                else if (game.HomeScore < game.AwayScore)
                {
                    winLossTies[game.AwayTeam].Wins++;
                    winLossTies[game.HomeTeam].Losses++;
                }
                else
                {
                    winLossTies[game.HomeTeam].Ties++;
                    winLossTies[game.AwayTeam].Ties++;
                }
            }
            return winLossTies;
        }

        public static Dictionary<BasicTeamInfo, int> GetSeasonDivisionRankings(IReadOnlyList<BasicTeamInfo> teams,
            IReadOnlyList<GameRecord> seasonGames)
        {
            var winLossTies = GetWinLossTies(teams, seasonGames.Where(g => g.GameType == GameType.RegularSeason));
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

        public static double SampleNormalDistribution(double mean, double standardDeviation, System.Random random)
        {
            standardDeviation = Math.Abs(standardDeviation);
            
            if (!distributionCache.ContainsKey((mean, standardDeviation)))
            {
                distributionCache.Add((mean, standardDeviation), new Normal(mean, standardDeviation, random));
            }
            
            var normalDistribution = distributionCache[(mean, standardDeviation)];

            return normalDistribution.Sample();
        }

        public static double SampleNormalDistribution(NormalDistributionParameters parameters, double value, System.Random random) =>
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

        public static double AddYardsForTeam(double startYard, double addend, GameTeam team)
        {
            return team switch
            {
                GameTeam.Away => startYard - addend,
                GameTeam.Home => startYard + addend,
                _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
            };
        }

        public static PlayContext CreateInitialPlayContext(IRandom random,
            GameRecord gameRecord,
            double startWindSpeedStddev,
            double airTemperature)
        {
            return new PlayContext(
                Version: 0L,
                NextState: PlayEvaluationState.Start,
                Environment: null,
                StateHistory: ImmutableList<StateHistoryEntry>.Empty,
                AdditionalParameters: ImmutableList<AdditionalParameter<object>>.Empty,
                BaseWindDirection: random.NextDouble() * 360d,
                BaseWindSpeed: random.SampleNormalDistribution(gameRecord.Stadium.AverageWindSpeed, startWindSpeedStddev),
                AirTemperature: airTemperature,
                CoinFlipWinner: GameTeam.Home,
                TeamWithPossession: GameTeam.Home,
                AwayScore: 0,
                HomeScore: 0,
                PeriodNumber: 1,
                SecondsLeftInPeriod: Constants.SecondsPerQuarter,
                ClockRunning: false,
                HomeTimeoutsRemaining: 3,
                AwayTimeoutsRemaining: 3,
                LineOfScrimmage: 50,
                LineToGain: null,
                NextPlay: NextPlayKind.Kickoff,
                DriveStartingFieldPosition: 50,
                DriveStartingPeriodNumber: 1,
                DriveStartingSecondsLeftInPeriod: Constants.SecondsPerQuarter,
                LastPlayDescriptionTemplate: "Gameplay loop initializing.",
                PossessionOnPlay: PossessionOnPlay.None,
                TeamCallingTimeout: null);
        }

        public static double GetTemperatureForGame(GameRecord gameRecord,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            IRandom random)
        {
            var stadium = gameRecord.Stadium;
            var month = gameRecord.KickoffTime.Month;
            var averageTemperatures = stadium.AverageTemperatures
                .Split(',')
                .Select(double.Parse);
            double averageTemperatureThisMonth;
            if (month >= 8)
            {
                // Indexes 0-4 correspond to August-December
                averageTemperatureThisMonth = averageTemperatures.ElementAt(month - 8);
            }
            else if (month <= 2)
            {
                // Indexes 5-6 correspond to January-February
                averageTemperatureThisMonth = averageTemperatures.ElementAt(month + 4);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Invalid kickoff month {month} for game ID {gameRecord.GameID}.");
            }

            var temperatureStddev = physicsParams["StartTemperatureStddev"].Value;
            return random.SampleNormalDistribution(averageTemperatureThisMonth, temperatureStddev);
        }

        public static TeamStrengthSet EstimateStrengthSetForTeam(Team team, Team requestingTeam,
            GameRecord gameRecord,
            IRandom random,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            double[] strengths = [
                team.RunningOffenseStrength,
                team.RunningDefenseStrength,
                team.PassingOffenseStrength,
                team.PassingDefenseStrength,
                team.OffensiveLineStrength,
                team.DefensiveLineStrength,
                team.KickingStrength,
                team.FieldGoalStrength,
                team.KickReturnStrength,
                team.KickDefenseStrength,
                team.ClockManagementStrength
            ];

            for (var i = 0; i < strengths.Length; i++)
            {
                var strengthRange = random.SampleNormalDistribution(physicsParams["StrengthEstimatorOffsetMean"].Value,
                    physicsParams["StrengthEstimatorOffsetStddev"].Value);
                strengthRange += team.Disposition switch
                {
                    TeamDisposition.UltraConservative => physicsParams["StrengthEstimatorUltraConservativeAdjustment"].Value,
                    TeamDisposition.Conservative => physicsParams["StrengthEstimatorConservativeAdjustment"].Value,
                    TeamDisposition.Insane => physicsParams["StrengthEstimatorInsaneAdjustment"].Value,
                    TeamDisposition.UltraInsane => physicsParams["StrengthEstimatorUltraInsaneAdjustment"].Value,
                    _ => throw new InvalidOperationException($"Team {team.TeamName} has invalid disposition {team.Disposition}.")
                };
                var rangeSample = random.NextDouble() * strengthRange;
                var shouldAdjustDownward = random.Chance(0.5);
                var strengthMultiplier = shouldAdjustDownward
                    ? 1.0 - rangeSample
                    : 1.0 + rangeSample;
                strengths[i] *= strengthMultiplier;
            }

            return new TeamStrengthSet
            {
                IsEstimate = true,
                Team = team.Equals(gameRecord.HomeTeam) ? GameTeam.Home : GameTeam.Away,
                StrengthSetKind = team.Equals(requestingTeam)
                    ? StrengthSetKind.TeamOfItself
                    : StrengthSetKind.TeamOfOpponent,
                RunningOffenseStrength = strengths[0],
                RunningDefenseStrength = strengths[1],
                PassingOffenseStrength = strengths[2],
                PassingDefenseStrength = strengths[3],
                OffensiveLineStrength = strengths[4],
                DefensiveLineStrength = strengths[5],
                KickingStrength = strengths[6],
                FieldGoalStrength = strengths[7],
                KickReturnStrength = strengths[8],
                KickDefenseStrength = strengths[9],
                ClockManagementStrength = strengths[10]
            };
        }

        // Source - https://stackoverflow.com/a/1674779
        // Posted by Jon Skeet, modified by community. See post 'Timeline' for change history
        // Retrieved 2026-01-20, License - CC BY-SA 2.5

        public static List<T> IntersectAll<T>(IEnumerable<IEnumerable<T>> lists)
        {
            HashSet<T>? hashSet = null;
            foreach (var list in lists)
            {
                if (hashSet == null)
                {
                    hashSet = [.. list];
                }
                else
                {
                    hashSet.IntersectWith(list);
                }
            }
            return hashSet == null ? [] : [.. hashSet];
        }

    }
}
