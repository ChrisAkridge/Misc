using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class InitializeWildCardRoundStepTests
    {
        [Fact]
        public void Run_InitializesWildCardRound()
        {
            // Arrange
            List<Team> teams = SeedData.TeamSeedData();
            for (int i = 0; i < teams.Count; i++)
            {
                Team? team = teams[i];
                team.TeamID = i + 1;    // usually we count on the database to assign IDs
                TestHelpers.SetRandomStrengths(team);
            }

            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetTeams()).Returns(teams);

            var randomFactory = new RandomFactory();
            var seasonGames = GetAndPlayRegularSeasonGames(
                repository.Object.GetTeams(),
                randomFactory,
                1
            );
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetGameRecordsForSeasonByGameType(1, GameType.RegularSeason))
                .Returns(seasonGames);
            
            var receivedGames = new List<GameRecord>();
            repository.Setup(r => r.AddGameRecords(It.IsAny<IReadOnlyList<GameRecord>>()))
                .Callback<IReadOnlyList<GameRecord>>(receivedGames.AddRange);
            var receivedSeeds = new List<TeamPlayoffSeed>();
            repository.Setup(r => r.AddTeamPlayoffSeeds(It.IsAny<IEnumerable<TeamPlayoffSeed>>()))
                .Callback<IEnumerable<TeamPlayoffSeed>>(s =>
                {
                    receivedSeeds.AddRange(s);
                });

            var mockRandomFactory = new Mock<IRandomFactory>();
            var mockRandom = new Mock<IRandom>();
            mockRandomFactory.Setup(rf => rf.Create(It.IsAny<int>())).Returns(mockRandom.Object);

            // Act
            var step = InitializeWildCardRoundStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = mockRandomFactory.Object,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    PlayerFactory = null!
                }
            });

            // Assert
            Assert.Equal(SystemState.LoadGame, step.NextState);
            Assert.Equal(8, receivedGames.Count);
            Assert.Equal(16, receivedSeeds.Count);
            Assert.Equal(8, receivedSeeds.GroupBy(s => s.Seed).Count());

            // Playoff seeds using the random scheduling fixed seed:
            // AFC: #1 Chargers, #2 Texans, #3 Dolphins, #4 Penguins, #5 Browns, #6 Titans, #7 Bills, #8 Patriots
            // NFC: #1 Cowboys, #2 Vikings, #3 Saints, #4 Seahawks, #5 Wasps, #6 Eagles, #7 Panthers, #8 Packers
            var teamNamesByID = teams.ToDictionary(t => t.TeamID, t => t.TeamName);
            var seededTeams = receivedSeeds
                .OrderBy(s => s.Seed)
                .Select(s => (s.Seed, teamNamesByID[s.TeamID]))
                .ToArray();
            Assert.Equal(new[]
            {
                (1, "Chargers"),
                (1, "Cowboys"),
                (2, "Texans"),
                (2, "Vikings"),
                (3, "Dolphins"),
                (3, "Saints"),
                (4, "Penguins"),
                (4, "Seahawks"),
                (5, "Browns"),
                (5, "Wasps"),
                (6, "Titans"),
                (6, "Eagles"),
                (7, "Bills"),
                (7, "Panthers"),
                (8, "Patriots"),
                (8, "Packers")
            }, seededTeams);

            // #8 @ #1, #7 @ #2, #6 @ #3, #5 @ #4
            var expectedMatchups = new[]
            {
                ("Patriots", "Chargers"),
                ("Bills", "Texans"),
                ("Titans", "Dolphins"),
                ("Browns", "Penguins"),
                ("Packers", "Cowboys"),
                ("Panthers", "Vikings"),
                ("Eagles", "Saints"),
                ("Wasps", "Seahawks")
            };

            var dateOfLastGame = seasonGames.Max(g => g.KickoffTime).AtMidnight();
            var wildCardSaturday = dateOfLastGame.AtMidnight().AddDays(5);
            var wildCardSunday = dateOfLastGame.AtMidnight().AddDays(6);
            var wildCardTimes = new[]
            {
                wildCardSaturday.AddHours(16).AddMinutes(25),
                wildCardSaturday.AddHours(20).AddMinutes(20),
                wildCardSunday.AddHours(16).AddMinutes(25),
                wildCardSunday.AddHours(20).AddMinutes(20)
            };

            // Assert game properties
            var stadiumIDsByTeam = teams.ToDictionary(t => t.TeamID, t => t.HomeStadiumID);

            Assert.All(receivedGames, g => Assert.Equal(1, g.SeasonRecordID));
            Assert.All(receivedGames, g => Assert.Equal(GameType.Postseason, g.GameType));
            foreach (var (expectedAway, expectedHome) in expectedMatchups)
            {
                var game = receivedGames.Single(g =>
                    teamNamesByID[g.AwayTeamID] == expectedAway &&
                    teamNamesByID[g.HomeTeamID] == expectedHome);
                var gameIndex = Array.IndexOf(expectedMatchups, (expectedAway, expectedHome));
                // Games 0 and 4 get time slot 0, games 1 and 5 get time slot 1, etc.
                var wildCardTimeIndex = gameIndex % 4;
                Assert.Equal(wildCardTimes[wildCardTimeIndex], game.KickoffTime);
                Assert.Equal(stadiumIDsByTeam[game.HomeTeamID], game.StadiumID);
            }
            Assert.All(receivedGames, g => Assert.Equal(18, g.WeekNumber));
            Assert.All(receivedGames, g => Assert.False(g.GameComplete));
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.AwayTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.AwayTeamID)));
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.HomeTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.HomeTeamID)));
        }

        // Helpers
        private static IReadOnlyList<GameRecord> GetAndPlayRegularSeasonGames(IReadOnlyList<Team> teams, IRandomFactory randomFactory,
            int seasonRecordID)
        {
            var scheduleGenerator = new ScheduleGenerator3(
                [.. teams.Select(t => new BasicTeamInfo(t))],
                randomFactory
            );
            var games = scheduleGenerator.GenerateScheduleForYear(
                2023,
                teams.ToDictionary(t => new BasicTeamInfo(t), t => t),
                null,
                null,
                out _
            )
                .Where(g => g.GameType == GameType.RegularSeason)
                .ToArray();

            if (games.Length != 320)
            {
                throw new InvalidOperationException("Expected 320 regular season games.");
            }

            var random = randomFactory.Create(Helpers.SchedulingRandomSeed);
            foreach (var game in games)
            {
                game.SeasonRecordID = seasonRecordID;
                var quarterBoxScores = Enumerable.Range(0, 8)
                    .Select(q => random.Choice([0, 3, 6, 7, 8]))
                    .Select((s, i) => new QuarterBoxScore
                    {
                        GameRecordID = game.GameID,
                        QuarterNumber = ((int)Math.Ceiling(i / 2d)) + 1,
                        Score = s,
                        Team = i % 2 == 0 ? GameTeam.Away : GameTeam.Home
                    });
                if (quarterBoxScores.Where(q => q.Team == GameTeam.Home).Sum(q => q.Score) ==
                    quarterBoxScores.Where(q => q.Team == GameTeam.Away).Sum(q => q.Score))
                {
                    // Add an overtime period
                    quarterBoxScores = quarterBoxScores.Append(new QuarterBoxScore
                    {
                        GameRecordID = game.GameID,
                        QuarterNumber = 5,
                        Score = random.Choice([0, 3, 6, 7, 8]),
                        Team = GameTeam.Away
                    })
                    .Append(new QuarterBoxScore
                    {
                        GameRecordID = game.GameID,
                        QuarterNumber = 5,
                        Score = random.Choice([0, 3, 6, 7, 8]),
                        Team = GameTeam.Home
                    });
                }
                game.QuarterBoxScores.AddRange(quarterBoxScores);
                game.GameComplete = true;
            }

            return games;
        }
    }
}
