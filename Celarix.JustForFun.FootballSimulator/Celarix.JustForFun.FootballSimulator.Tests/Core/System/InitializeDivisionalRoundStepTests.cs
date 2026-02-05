using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class InitializeDivisionalRoundStepTests
    {
        [Fact]
        public void Run_InitializesDivisionalRound()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason())
                .Returns(new SeasonRecord { SeasonRecordID = 1 });

            var baseKickoffTime = DateTimeOffset.Parse("2026-01-29T00:00:00Z");
            var wildCardGames = new List<GameRecord>();
            for (int i = 1; i <= 8; i++)
            {
                Team team = new()
                {
                    TeamID = i,
                    Conference = (i <= 4) ? Conference.AFC : Conference.NFC,
                    HomeStadiumID = i
                };
                TestHelpers.SetRandomStrengths(team);
                var game = new GameRecord
                {
                    AwayTeam = team,
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ],
                    KickoffTime = baseKickoffTime.AddHours(i),
                    GameComplete = true
                };
                wildCardGames.Add(game);
            }
            repository.Setup(r => r.GetPlayoffGamesForSeason(1, PlayoffRound.WildCard))
                .Returns(wildCardGames);

            var teamPlayoffSeeds = new List<TeamPlayoffSeed>();
            for (int i = 1; i <= 8; i++)
            {
                var seed = new TeamPlayoffSeed
                {
                    TeamID = i,
                    Seed = i % 4 == 0 ? 4 : i % 4,
                    SeasonRecordID = 1
                };
                teamPlayoffSeeds.Add(seed);
            }
            repository.Setup(r => r.GetPlayoffSeedsForSeason(1))
                .Returns(teamPlayoffSeeds);

            var receivedGames = new List<GameRecord>();
            repository.Setup(r => r.AddGameRecords(It.IsAny<IEnumerable<GameRecord>>()))
                .Callback<IEnumerable<GameRecord>>(receivedGames.AddRange);

            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeDivisionalRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    SummaryWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeDivisionalRoundStep.Run(context);

            // Assert
            Assert.Equal(4, receivedGames.Count);
            Assert.All(receivedGames, g => Assert.Equal(1, g.SeasonRecordID));
            Assert.All(receivedGames, g => Assert.Equal(GameType.Postseason, g.GameType));

            // Teams 1-4 are the AFC teams, 5-8 are the NFC teams
            // Away teams won all wild card games with a score of 3-0
            // #1 seed is 1/5, #2 seed is 2/6, #3 seed is 3/7, #4 seed is 4/8
            // Divisional games should be:
            // AFC: 4 @ 1, 3 @ 2
            // NFC: 8 @ 5, 7 @ 6
            var afcGames = receivedGames.Where(g => g.AwayTeamID <= 4).ToList();
            var nfcGames = receivedGames.Where(g => g.AwayTeamID >= 5).ToList();
            Assert.Equal(2, afcGames.Count);
            Assert.Equal(2, nfcGames.Count);
            // Do a trick here to prove the same thing as a game-by-game test
            // 1. All teams 1-8 appear in the games
            for (var i = 1; i <= 8; i++)
            {
                Assert.Contains(receivedGames, g => g.AwayTeamID == i || g.HomeTeamID == i);
            }
            // 2. All team IDs for AFC teams sum to 5
            foreach (var afcGame in afcGames)
            {
                Assert.Equal(5, afcGame.AwayTeamID + afcGame.HomeTeamID);
            }
            // 3. All team IDs for NFC teams sum to 13
            foreach (var nfcGame in nfcGames)
            {
                Assert.Equal(13, nfcGame.AwayTeamID + nfcGame.HomeTeamID);
            }
            // Dumb? Yes. Correct? Also yes.

            DateTimeOffset[] expectedKickoffTimes =
            [
                baseKickoffTime.AddDays(7).AtMidnight().AddHours(16).AddMinutes(25),
                baseKickoffTime.AddDays(7).AtMidnight().AddHours(20).AddMinutes(20),
                baseKickoffTime.AddDays(8).AtMidnight().AddHours(16).AddMinutes(25),
                baseKickoffTime.AddDays(8).AtMidnight().AddHours(20).AddMinutes(20),
            ];
            foreach (var expectedTime in expectedKickoffTimes)
            {
                Assert.Single(receivedGames, g => g.KickoffTime == expectedTime);
            }

            // Home stadiums are the home team IDs
            Assert.All(receivedGames, g => Assert.Equal(g.HomeTeamID, g.StadiumID));
            Assert.All(receivedGames, g => Assert.Equal(19, g.WeekNumber));
            Assert.All(receivedGames, g => Assert.False(g.GameComplete));

            var teams = wildCardGames.Select(g => g.AwayTeam).ToArray();
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.AwayTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.AwayTeamID)));
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.HomeTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.HomeTeamID)));

            Assert.Equal(SystemState.LoadGame, step.NextState);
        }

        [Fact]
        public void Run_ThrowsWhenNoSeasonFound()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason())
                .Returns((SeasonRecord?)null);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeDivisionalRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    SummaryWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => InitializeDivisionalRoundStep.Run(context));
            Assert.Equal("No season records found in database.", exception.Message);
        }

        [Fact]
        public void Run_ThrowsWhenNotEnoughWildCardGames()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason())
                .Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetPlayoffGamesForSeason(1, PlayoffRound.WildCard))
                .Returns([]); // No games
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeDivisionalRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    SummaryWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => InitializeDivisionalRoundStep.Run(context));
            Assert.Equal("There should be exactly 8 wild card games to proceed to the divisional round.", exception.Message);
        }

        [Fact]
        public void Run_ThrowsOnIncompleteWildCardGames()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason())
                .Returns(new SeasonRecord { SeasonRecordID = 1 });
            var wildCardGames = new List<GameRecord>();
            for (int i = 1; i <= 8; i++)
            {
                var game = new GameRecord
                {
                    GameComplete = i % 2 == 0 // Half complete, half not
                };
                wildCardGames.Add(game);
            }
            repository.Setup(r => r.GetPlayoffGamesForSeason(1, PlayoffRound.WildCard))
                .Returns(wildCardGames);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeDivisionalRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    SummaryWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => InitializeDivisionalRoundStep.Run(context));
            Assert.Equal("All wild card games must be complete to proceed to the divisional round.", exception.Message);
        }

        [Fact]
        public void Run_ThrowsOnWildCardGameTies()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason())
                .Returns(new SeasonRecord { SeasonRecordID = 1 });
            var wildCardGames = new List<GameRecord>();
            for (int i = 1; i <= 8; i++)
            {
                var game = new GameRecord
                {
                    GameComplete = true,
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Home,
                            Score = 3
                        },
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ]
                };
                wildCardGames.Add(game);
            }
            repository.Setup(r => r.GetPlayoffGamesForSeason(1, PlayoffRound.WildCard))
                .Returns(wildCardGames);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeDivisionalRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    SummaryWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => InitializeDivisionalRoundStep.Run(context));
            Assert.Equal("No wild card games can end in a tie to proceed to the divisional round.", exception.Message);
        }
    }
}
