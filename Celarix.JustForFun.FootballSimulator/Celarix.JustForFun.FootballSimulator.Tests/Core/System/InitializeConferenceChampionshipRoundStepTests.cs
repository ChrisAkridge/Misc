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
    public class InitializeConferenceChampionshipRoundStepTests
    {
        [Fact]
        public void Run_CreatesConferenceChampionshipGames()
        {
            // Arrange
            var firstKickoffTime = DateTimeOffset.Parse("2026-01-29T00:00:00Z");

            Team[] teams = [
                new Team { TeamID = 1, Conference = Conference.AFC, HomeStadiumID = 1 },
                new Team { TeamID = 2, Conference = Conference.AFC, HomeStadiumID = 2 },
                new Team { TeamID = 3, Conference = Conference.NFC, HomeStadiumID = 3 },
                new Team { TeamID = 4, Conference = Conference.NFC, HomeStadiumID = 4 },
            ];

            foreach (var team in teams) { TestHelpers.SetRandomStrengths(team); }

            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetPlayoffGamesForSeason(It.IsAny<int>(), It.IsAny<PlayoffRound>())).Returns(
            [
                new GameRecord
                {
                    GameID = 1,
                    GameComplete = true,
                    AwayTeam = teams[0],
                    KickoffTime = firstKickoffTime,
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ]
                },
                new GameRecord
                {
                    GameID = 2,
                    GameComplete = true,
                    KickoffTime = firstKickoffTime.AddHours(1d),
                    AwayTeam = teams[1],
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ]
                },
                new GameRecord
                {
                    GameID = 3,
                    GameComplete = true,
                    KickoffTime = firstKickoffTime.AddHours(2d),
                    AwayTeam = teams[2],
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ]
                },
                new GameRecord
                {
                    GameID = 4,
                    GameComplete = true,
                    KickoffTime = firstKickoffTime.AddHours(3d),
                    AwayTeam = teams[3],
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 3
                        }
                    ]
                },
            ]);
            repository.Setup(r => r.GetPlayoffSeedsForSeason(It.IsAny<int>())).Returns(
            [
                new TeamPlayoffSeed { TeamID = 1, Seed = 1 },
                new TeamPlayoffSeed { TeamID = 2, Seed = 2 },
                new TeamPlayoffSeed { TeamID = 3, Seed = 1 },
                new TeamPlayoffSeed { TeamID = 4, Seed = 2 },
            ]);

            var receivedGames = new List<GameRecord>();
            repository.Setup(r => r.AddGameRecords(It.IsAny<IEnumerable<GameRecord>>())).Callback<IEnumerable<GameRecord>>(receivedGames.AddRange);

            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeConferenceChampionshipRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeConferenceChampionshipRoundStep.Run(context);

            // Assert
            repository.Verify(r => r.AddGameRecords(It.IsAny<IEnumerable<GameRecord>>()), Times.Once);
            repository.Verify(r => r.SaveChanges(), Times.Once);
            Assert.Equal(2, receivedGames.Count);
            var afcGame = receivedGames.Single(g => g.HomeTeamID == 1);
            var nfcGame = receivedGames.Single(g => g.HomeTeamID == 3);
            Assert.Equal(1, afcGame.SeasonRecordID);
            Assert.Equal(1, nfcGame.SeasonRecordID);
            Assert.All(receivedGames, g => Assert.Equal(GameType.Postseason, g.GameType));
            Assert.Equal(2, afcGame.AwayTeamID);
            Assert.Equal(1, afcGame.HomeTeamID);
            Assert.Equal(4, nfcGame.AwayTeamID);
            Assert.Equal(3, nfcGame.HomeTeamID);
            Assert.Equal(firstKickoffTime.AddDays(8).AddHours(16).AddMinutes(25), afcGame.KickoffTime);
            Assert.Equal(firstKickoffTime.AddDays(8).AddHours(20).AddMinutes(20), nfcGame.KickoffTime);
            Assert.Equal(1, afcGame.StadiumID);
            Assert.Equal(3, nfcGame.StadiumID);
            Assert.All(receivedGames, g => Assert.Equal(20, g.WeekNumber));
            Assert.All(receivedGames, g => Assert.False(g.GameComplete));
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.AwayTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.AwayTeamID)));
            Assert.All(receivedGames, g => TestHelpers.AssertStrengthJSON(g.HomeTeamStrengthsAtKickoffJSON, teams.Single(t => t.TeamID == g.HomeTeamID)));

            Assert.Equal(SystemState.LoadGame, step.NextState);
        }

        [Fact]
        public void Run_ThrowsWhenDivisionalRoundNotComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetPlayoffGamesForSeason(It.IsAny<int>(), It.IsAny<PlayoffRound>())).Returns(
            [
                new GameRecord
                {
                    GameID = 1,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 2,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 3,
                    GameComplete = true
                }
            ]);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeConferenceChampionshipRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                InitializeConferenceChampionshipRoundStep.Run(context));
        }

        [Fact]
        public void Run_ThrowsWhenNotAllDivisionalGamesComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetPlayoffGamesForSeason(It.IsAny<int>(), It.IsAny<PlayoffRound>())).Returns(
            [
                new GameRecord
                {
                    GameID = 1,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 2,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 3,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 4,
                    GameComplete = false
                },
            ]);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeConferenceChampionshipRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                InitializeConferenceChampionshipRoundStep.Run(context));
        }

        [Fact]
        public void Run_ThrowsWhenDivisionalRoundTiesExist()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord { SeasonRecordID = 1 });
            repository.Setup(r => r.GetPlayoffGamesForSeason(It.IsAny<int>(), It.IsAny<PlayoffRound>())).Returns(
            [
                new GameRecord
                {
                    GameID = 1,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 2,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 3,
                    GameComplete = true
                },
                new GameRecord
                {
                    GameID = 4,
                    GameComplete = true,
                    QuarterBoxScores =
                    [
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Away,
                            Score = 10
                        },
                        new QuarterBoxScore
                        {
                            Team = GameTeam.Home,
                            Score = 10
                        }
                    ]
                },
            ]);
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InitializeConferenceChampionshipRound,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                InitializeConferenceChampionshipRoundStep.Run(context));
        }
    }
}
