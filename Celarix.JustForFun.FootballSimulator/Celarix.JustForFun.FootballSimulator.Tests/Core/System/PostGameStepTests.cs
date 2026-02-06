using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using Moq;
using System;
using System.Collections.Generic;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class PostGameStepTests
    {
        [Fact]
        public void Run_WithRegularSeasonGame_SetsCorrectNextState()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 15, // Regular season week
                AwayTeamID = 1,
                HomeTeamID = 2,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var injuryRecoveries = new List<InjuryRecovery>
            {
                new InjuryRecovery { InjuryRecoveryID = 1, Recovered = false, Strength = "" },
                new InjuryRecovery { InjuryRecoveryID = 2, Recovered = false, Strength = "" }
            };

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(1, 2, It.IsAny<DateTimeOffset>()))
                          .Returns(injuryRecoveries);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var context = new SystemContext(1, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.Equal(SystemState.WriteSummaryForGame, result.NextState);
            Assert.True(gameRecord.GameComplete);
            Assert.All(injuryRecoveries, ir => Assert.True(ir.Recovered));
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
            mockRepository.Verify(r => r.GetInjuryRecoveriesForGame(1, 2, gameRecord.KickoffTime), Times.Once);
        }

        [Fact]
        public void Run_WithSuperBowlGame_SetsCorrectNextState()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 22, // Super Bowl week
                AwayTeamID = 3,
                HomeTeamID = 4,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var injuryRecoveries = new List<InjuryRecovery>
            {
                new InjuryRecovery { InjuryRecoveryID = 3, Recovered = false, Strength = "" }
            };

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(3, 4, It.IsAny<DateTimeOffset>()))
                          .Returns(injuryRecoveries);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var context = new SystemContext(1, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.Equal(SystemState.WriteSummaryForSeason, result.NextState);
            Assert.True(gameRecord.GameComplete);
            Assert.All(injuryRecoveries, ir => Assert.True(ir.Recovered));
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
            mockRepository.Verify(r => r.GetInjuryRecoveriesForGame(3, 4, gameRecord.KickoffTime), Times.Once);
        }

        [Fact]
        public void Run_WithNoInjuryRecoveries_StillCallsSaveChanges()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 10,
                AwayTeamID = 5,
                HomeTeamID = 6,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var emptyInjuryRecoveries = new List<InjuryRecovery>();

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(5, 6, It.IsAny<DateTimeOffset>()))
                          .Returns(emptyInjuryRecoveries);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var context = new SystemContext(1, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.Equal(SystemState.WriteSummaryForGame, result.NextState);
            Assert.True(gameRecord.GameComplete);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
            mockRepository.Verify(r => r.GetInjuryRecoveriesForGame(5, 6, gameRecord.KickoffTime), Times.Once);
        }

        [Fact]
        public void Run_WithMultipleInjuryRecoveries_MarksAllAsRecovered()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 8,
                AwayTeamID = 7,
                HomeTeamID = 8,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var injuryRecoveries = new List<InjuryRecovery>
            {
                new InjuryRecovery { InjuryRecoveryID = 1, Recovered = false, Strength = "" },
                new InjuryRecovery { InjuryRecoveryID = 2, Recovered = false, Strength = "" },
                new InjuryRecovery { InjuryRecoveryID = 3, Recovered = false, Strength = "" },
                new InjuryRecovery { InjuryRecoveryID = 4, Recovered = false, Strength = "" }
            };

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(7, 8, It.IsAny<DateTimeOffset>()))
                          .Returns(injuryRecoveries);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var context = new SystemContext(1, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.True(gameRecord.GameComplete);
            Assert.All(injuryRecoveries, ir => Assert.True(ir.Recovered));
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_WithMixedInjuryRecoveries_OnlyMarksUnrecoveredOnes()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 12,
                AwayTeamID = 9,
                HomeTeamID = 10,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var injuryRecoveries = new List<InjuryRecovery>
            {
                new InjuryRecovery { InjuryRecoveryID = 1, Recovered = false, Strength = "" },
                new InjuryRecovery { InjuryRecoveryID = 2, Recovered = true, Strength = "" }, // Already recovered
                new InjuryRecovery { InjuryRecoveryID = 3, Recovered = false, Strength = "" }
            };

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(9, 10, It.IsAny<DateTimeOffset>()))
                          .Returns(injuryRecoveries);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var context = new SystemContext(1, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.True(gameRecord.GameComplete);
            // All should be marked as recovered after the step
            Assert.All(injuryRecoveries, ir => Assert.True(ir.Recovered));
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_PreservesContextVersionAndEnvironment()
        {
            // Arrange
            var gameRecord = new GameRecord
            {
                WeekNumber = 5,
                AwayTeamID = 11,
                HomeTeamID = 12,
                KickoffTime = DateTimeOffset.Now,
                GameComplete = false
            };

            var mockRepository = new Mock<IFootballRepository>();
            mockRepository.Setup(r => r.GetInjuryRecoveriesForGame(11, 12, It.IsAny<DateTimeOffset>()))
                          .Returns([]);

            var environment = new SystemEnvironment
            {
                FootballRepository = mockRepository.Object,
                RandomFactory = Mock.Of<IRandomFactory>(),
                PlayerFactory = null!,
                SummaryWriter = null!,
                DebugContextWriter = null!,
                CurrentGameRecord = gameRecord,
                EventBus = Mock.Of<IEventBus>()
            };

            var originalVersion = 42L;
            var context = new SystemContext(originalVersion, SystemState.PostGame, environment);

            // Act
            var result = PostGameStep.Run(context);

            // Assert
            Assert.Equal(originalVersion, result.Version);
            Assert.Same(environment, result.Environment);
        }
    }
}