using Celarix.JustForFun.FootballSimulator.Core;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using MathNet.Numerics.Distributions;
using Microsoft.EntityFrameworkCore;
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

        public static IEnumerable<BasicTeamInfo> GetTeamsInDivision(IEnumerable<BasicTeamInfo> teams, Conference conference, Division division) =>
			teams.Where(t => t.Conference == conference && t.Division == division);

		public static GameTeam OtherTeam(GameTeam team) =>
            team switch
            {
                GameTeam.Home => GameTeam.Away,
                GameTeam.Away => GameTeam.Home,
                _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
            };

        public static DriveDirection TowardOpponentEndzone(GameTeam team) =>
            team switch
            {
                GameTeam.Home => DriveDirection.TowardAwayEndzone,
                GameTeam.Away => DriveDirection.TowardHomeEndzone,
                _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
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
            var normalDistribution = new Normal(mean, standardDeviation, random);
            return normalDistribution.Sample();
        }

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

        internal static PlayContext CreateInitialPlayContext(IRandom random,
            GameRecord gameRecord,
            double startWindSpeedStddev,
            double airTemperature)
        {
            if (gameRecord.Stadium == null)
            {
                throw new InvalidOperationException(
                    $"Stadium is null for game ID {gameRecord.GameID} when creating initial play context.");
            }

            return new PlayContext(
                Version: 0L,
                NextState: PlayEvaluationState.Start,
                Environment: null,
                StateHistory: [],
                AdditionalParameters: [],
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
                PlayInvolvement: new PlayInvolvement(
                    InvolvesOffenseRun: false,
                    InvolvesDefenseRun: false,
                    OffensivePlayersInvolved: 0,
                    DefensivePlayersInvolved: 0,
                    InvolvesKick: false,
                    InvolvesOffensePass: false),
                LineOfScrimmage: 50,
                LineToGain: null,
                NextPlay: NextPlayKind.Kickoff,
                DriveStartingFieldPosition: 50,
                DriveStartingPeriodNumber: 1,
                DriveStartingSecondsLeftInPeriod: Constants.SecondsPerQuarter,
                DriveResult: null,
                LastPlayDescriptionTemplate: "Gameplay loop initializing.",
                AwayScoredThisPlay: false,
                HomeScoredThisPlay: false,
                PossessionOnPlay: PossessionOnPlay.None,
                TeamCallingTimeout: null);
        }

        public static double GetTemperatureForGame(GameRecord gameRecord,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            IRandom random)
        {
            var stadium = gameRecord.Stadium ?? throw new InvalidOperationException(
                    $"Stadium is null for game ID {gameRecord.GameID} when getting temperature for game.");
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

        internal static TeamStrengthSet EstimateStrengthSetForTeam(Team team, Team requestingTeam,
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


        public static string CapitalizeLastName(string lastName)
        {
            // Ensure the name is title case
            var titleCased = char.ToUpper(lastName[0]) + lastName[1..].ToLower();

            // If the name has an apostrophe or a hyphen, capitalize the letter after it
            var capitalizeAfter = new[] { '\'', '-', ' ' };
            foreach (var separator in capitalizeAfter)
            {
                var separatorIndex = titleCased.IndexOf(separator);
                if (separatorIndex > 0 && separatorIndex < titleCased.Length - 1)
                {
                    titleCased = titleCased[..(separatorIndex + 1)] +
                        char.ToUpper(titleCased[separatorIndex + 1]) +
                        titleCased[(separatorIndex + 2)..];
                }
            }

            // If the name starts with "Mc", capitalize the letter after it
            if (titleCased.Length > 2 && titleCased.StartsWith("Mc"))
            {
                titleCased = "Mc" +
                    char.ToUpper(titleCased[2]) +
                    titleCased[3..];
            }

            return titleCased;
        }

        public static PlayInvolvement CreateInitialPlayInvolvement()
        {
            return new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 0,
                DefensivePlayersInvolved: 0
            );
        }

        internal static void RebuildStrengthsInDecisionParameters(GameContext context, IRandom random)
        {
            var decisionParameters = context.Environment.CurrentPlayContext!.Environment!.DecisionParameters!;
            var awayTeam = context.Environment.CurrentGameRecord!.AwayTeam;
            var homeTeam = context.Environment.CurrentGameRecord.HomeTeam;
            var gameRecord = context.Environment.CurrentGameRecord;
            var physicsParams = context.Environment.PhysicsParams;

            if (awayTeam == null)
            {
                throw new InvalidOperationException(
                    $"Away team is null for game ID {gameRecord.GameID} when rebuilding strengths in decision parameters.");
            }

            if (homeTeam == null)
            {
                throw new InvalidOperationException(
                    $"Home team is null for game ID {gameRecord.GameID} when rebuilding strengths in decision parameters.");
            }

            decisionParameters.AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(awayTeam, GameTeam.Away);
            decisionParameters.HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(homeTeam, GameTeam.Home);
            decisionParameters.AwayTeamEstimateOfAway = EstimateStrengthSetForTeam(awayTeam, awayTeam, gameRecord, random, physicsParams);
            decisionParameters.AwayTeamEstimateOfHome = EstimateStrengthSetForTeam(homeTeam, awayTeam, gameRecord, random, physicsParams);
            decisionParameters.HomeTeamEstimateOfAway = EstimateStrengthSetForTeam(awayTeam, homeTeam, gameRecord, random, physicsParams);
            decisionParameters.HomeTeamEstimateOfHome = EstimateStrengthSetForTeam(homeTeam, homeTeam, gameRecord, random, physicsParams);
        }

        internal static void SaveQuarterBoxScores(GameContext context)
        {
            var playContext = context.Environment.CurrentPlayContext!;
            var repository = context.Environment.FootballRepository;
            var gameRecordID = context.Environment.CurrentGameRecord!.GameID;
            var awayScoreInSavedQuarters = repository.GetScoreForTeamInGame(gameRecordID, GameTeam.Away);
            var homeScoreInSavedQuarters = repository.GetScoreForTeamInGame(gameRecordID, GameTeam.Home);
            var awayScoreInCurrentQuarter = playContext.AwayScore - awayScoreInSavedQuarters;
            var homeScoreInCurrentQuarter = playContext.HomeScore - homeScoreInSavedQuarters;

            repository.AddQuarterBoxScore(new QuarterBoxScore
            {
                GameRecordID = gameRecordID,
                QuarterNumber = playContext.PeriodNumber,
                Team = GameTeam.Away,
                Score = awayScoreInCurrentQuarter
            });

            repository.AddQuarterBoxScore(new QuarterBoxScore
            {
                GameRecordID = gameRecordID,
                QuarterNumber = playContext.PeriodNumber,
                Team = GameTeam.Home,
                Score = homeScoreInCurrentQuarter
            });
        }

        internal static double GetTemperatureForStadiumAndMonth(Stadium stadium, int month)
        {
            if (month is > 2 and < 8)
            {
                throw new ArgumentOutOfRangeException(nameof(month), $"Invalid month {month} for temperature lookup.");
            }

            var averageTemperatures = stadium.AverageTemperatures
                .Split(',')
                .Select(double.Parse);
            return month >= 8
                ? // Indexes 0-4 correspond to August-December
                averageTemperatures.ElementAt(month - 8)
                : // Indexes 5-6 correspond to January-February
                averageTemperatures.ElementAt(month + 4);
        }
    }
}
