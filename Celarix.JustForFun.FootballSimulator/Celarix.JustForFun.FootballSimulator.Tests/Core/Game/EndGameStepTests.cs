using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class EndGameStepTests
    {
        private static Team CreateTestTeam(int teamId, string cityName, string teamName, string abbreviation)
        {
            var team = new Team
            {
                TeamID = teamId,
                CityName = cityName,
                TeamName = teamName,
                Abbreviation = abbreviation,
                Disposition = TeamDisposition.Conservative
            };
            TestHelpers.SetFixedStrengths(team, 75.0);
            return team;
        }

        private static GameContext CreateTestGameContext(GameRecord gameRecord, Mock<IFootballRepository> mockRepository)
        {
            var physicsParams = new Dictionary<string, PhysicsParam>
            {
                { "TestParam", new PhysicsParam("TestParam", 1.0, "", "") }
            };

            var environment = new GameEnvironment
            {
                CurrentGameRecord = gameRecord,
                FootballRepository = mockRepository.Object,
                PhysicsParams = physicsParams,
                RandomFactory = new Mock<IRandomFactory>().Object,
                AwayActiveRoster = Array.Empty<PlayerRosterPosition>(),
                HomeActiveRoster = Array.Empty<PlayerRosterPosition>(),
                DebugContextWriter = null!,
                CurrentPlayContext = TestHelpers.EmptyPlayContext,
                EventBus = Mock.Of<IEventBus>()
            };

            return TestHelpers.EmptyGameContext with
            {
                Environment = environment
            };
        }

        private static void SetScores(GameRecord gameRecord, GameEnvironment gameEnvironment, int awayScore, int homeScore)
        {
            gameRecord.QuarterBoxScores =
            [
                new QuarterBoxScore
                {
                    Team = GameTeam.Away,
                    QuarterNumber = 1,
                    Score = awayScore
                },
                new QuarterBoxScore
                {
                    Team = GameTeam.Home,
                    QuarterNumber = 1,
                    Score = homeScore
                },
            ];

            gameEnvironment.CurrentPlayContext = gameEnvironment.CurrentPlayContext with
            {
                AwayScore = awayScore,
                HomeScore = homeScore
            };
        }

        [Fact]
        public void Run_ValidGameContext_CompletesGameInRepository()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam,
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            mockRepository.Verify(r => r.CompleteGame(123), Times.Once);
        }

        [Fact]
        public void Run_ValidGameContext_SetsTeamStrengthsForBothTeams()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            mockRepository.Verify(r => r.SetTeamStrengths(awayTeam, 1), Times.Once);
            mockRepository.Verify(r => r.SetTeamStrengths(homeTeam, 2), Times.Once);
        }

        [Fact]
        public void Run_ValidGameContext_SavesChangesToRepository()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_ValidGameContext_ReturnsContextWithEndGameState()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };
            
            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            Assert.Equal(GameState.EndGame, result.NextState);
        }

        [Fact]
        public void Run_ValidGameContext_VerifiesRepositoryMethodCallOrder()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };
            

            var mockRepository = new Mock<IFootballRepository>();
            var callSequence = new MockSequence();
            
            mockRepository.InSequence(callSequence).Setup(r => r.CompleteGame(123));
            mockRepository.InSequence(callSequence).Setup(r => r.SetTeamStrengths(awayTeam, 1));
            mockRepository.InSequence(callSequence).Setup(r => r.SetTeamStrengths(homeTeam, 2));
            mockRepository.InSequence(callSequence).Setup(r => r.SaveChanges());

            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            mockRepository.VerifyAll();
        }

        [Fact]
        public void Run_ValidGameContext_PreservesEnvironmentAndContext()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            Assert.Same(context.Environment, result.Environment);
            Assert.Equal(context.AwayTeamAcclimatedTemperature, result.AwayTeamAcclimatedTemperature);
            Assert.Equal(context.HomeTeamAcclimatedTemperature, result.HomeTeamAcclimatedTemperature);
            Assert.Equal(context.TeamWithPossession, result.TeamWithPossession);
            Assert.Equal(context.PlayCountOnDrive, result.PlayCountOnDrive);
        }

        [Fact]
        public void Run_ValidGameContext_IncrementsVersionNumber()
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 123,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository) with { Version = 42 };
            SetScores(gameRecord, context.Environment, 21, 14);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            Assert.Equal(43, result.Version);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(21, 14)]
        [InlineData(42, 35)]
        [InlineData(3, 6)]
        public void Run_DifferentScores_HandlesAllScenarios(int awayScore, int homeScore)
        {
            // Arrange
            var awayTeam = CreateTestTeam(1, "Away City", "Away Team", "AWY");
            var homeTeam = CreateTestTeam(2, "Home City", "Home Team", "HOM");
            
            var gameRecord = new GameRecord
            {
                GameID = 456,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam
            };

            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateTestGameContext(gameRecord, mockRepository);
            SetScores(gameRecord, context.Environment, awayScore, homeScore);

            // Act
            var result = EndGameStep.Run(context);

            // Assert
            Assert.Equal(GameState.EndGame, result.NextState);
            mockRepository.Verify(r => r.CompleteGame(456), Times.Once);
            mockRepository.Verify(r => r.SetTeamStrengths(awayTeam, 1), Times.Once);
            mockRepository.Verify(r => r.SetTeamStrengths(homeTeam, 2), Times.Once);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }
    }
}