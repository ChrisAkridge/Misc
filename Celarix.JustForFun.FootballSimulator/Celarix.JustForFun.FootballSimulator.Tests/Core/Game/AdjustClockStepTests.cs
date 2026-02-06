using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using Serilog;
using System.Collections.Generic;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class AdjustClockStepTests
    {
        private readonly Mock<IRandomFactory> _mockRandomFactory;
        private readonly Mock<IRandom> _mockRandom;
        private readonly Mock<ILogger> _mockLogger;
        private readonly Dictionary<string, PhysicsParam> _physicsParams;
        private readonly Team _awayTeam;
        private readonly Team _homeTeam;

        public AdjustClockStepTests()
        {
            _mockRandomFactory = new Mock<IRandomFactory>();
            _mockRandom = new Mock<IRandom>();
            _mockLogger = new Mock<ILogger>();
            
            _mockRandomFactory.Setup(rf => rf.Create()).Returns(_mockRandom.Object);
            
            // Setup basic physics parameters
            _physicsParams = new Dictionary<string, PhysicsParam>
            {
                ["PlayTimeSecondsMean"] = new PhysicsParam("PlayTimeSecondsMean", 6.0, "second", "seconds"),
                ["PlayTimeSecondsStddev"] = new PhysicsParam("PlayTimeSecondsStddev", 2.0, "second", "seconds"),
                ["TimeBetweenPlaysTwoMinuteDrill"] = new PhysicsParam("TimeBetweenPlaysTwoMinuteDrill", 15.0, "second", "seconds"),
                ["TimeBetweenPlaysHurryUp"] = new PhysicsParam("TimeBetweenPlaysHurryUp", 20.0, "second", "seconds"),
                ["TimeBetweenPlaysRelaxed"] = new PhysicsParam("TimeBetweenPlaysRelaxed", 35.0, "second", "seconds"),
                ["TimeBetweenPlaysClockChewing"] = new PhysicsParam("TimeBetweenPlaysClockChewing", 40.0, "second", "seconds"),
                ["StrengthEstimatorOffsetMean"] = new PhysicsParam("StrengthEstimatorOffsetMean", 0, "offset", "offset"),
                ["StrengthEstimatorOffsetStddev"] = new PhysicsParam("StrengthEstimatorOffsetStddev", 0.1, "offset", "offset"),
                ["StrengthEstimatorConservativeAdjustment"] = new PhysicsParam("StrengthEstimatorConservativeAdjustment", 0, "offset", "offset"),
                ["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInStandardZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay", 1.0, "multiple", "multiple")
            };

            // Setup Serilog mock for static logging calls
            Log.Logger = _mockLogger.Object;

            _awayTeam = new Team
            {
                CityName = "Greenwich",
                TeamName = "Greenwich Assertions",
                Abbreviation = "GAS",
                Disposition = TeamDisposition.Conservative
            };

            _homeTeam = new Team
            {
                CityName = "Springfield",
                TeamName = "Springfield Monads",
                Abbreviation = "SMD",
                Disposition = TeamDisposition.Conservative
            };
        }

        private GameContext CreateTestGameContext(PlayContext playContext)
        {
            GameContext gameContext = new(
                Version: 1L,
                NextState: GameState.AdjustClock,
                Environment: new GameEnvironment
                {
                    CurrentPlayContext = playContext,
                    PhysicsParams = _physicsParams,
                    RandomFactory = _mockRandomFactory.Object,
                    FootballRepository = null!,
                    DebugContextWriter = null!,
                    CurrentGameRecord = new GameRecord
                    {
                        AwayTeam = _awayTeam,
                        HomeTeam = _homeTeam
                    },
                    AwayActiveRoster = [],
                    HomeActiveRoster = [],
                    EventBus = Mock.Of<IEventBus>()
                },
                AwayTeamAcclimatedTemperature: 70.0,
                HomeTeamAcclimatedTemperature: 70.0,
                TeamWithPossession: GameTeam.Home,
                PlayCountOnDrive: 1
            );
            Helpers.RebuildStrengthsInDecisionParameters(gameContext, _mockRandom.Object);
            return gameContext;
        }

        private PlayContext CreateTestPlayContext(
            int secondsLeftInPeriod = 900,
            bool clockRunning = true,
            int homeTimeouts = 3,
            int awayTimeouts = 3,
            GameTeam? teamCallingTimeout = null)
        {
            TestHelpers.SetFixedStrengths(_awayTeam, 100);
            TestHelpers.SetFixedStrengths(_homeTeam, 100);

            return new PlayContext(
                Version: 1L,
                NextState: PlayEvaluationState.Start,
                AdditionalParameters: [],
                StateHistory: [],
                Environment: CreateMockPlayEnvironment(_mockRandom.Object, _awayTeam, _homeTeam),
                BaseWindDirection: 0.0,
                BaseWindSpeed: 0.0,
                AirTemperature: 70.0,
                CoinFlipWinner: GameTeam.Home,
                TeamWithPossession: GameTeam.Home,
                AwayScore: 0,
                HomeScore: 0,
                PeriodNumber: 1,
                SecondsLeftInPeriod: secondsLeftInPeriod,
                ClockRunning: clockRunning,
                HomeTimeoutsRemaining: homeTimeouts,
                AwayTimeoutsRemaining: awayTimeouts,
                PlayInvolvement: TestHelpers.EmptyPlayInvolvement,
                LineOfScrimmage: 25,
                LineToGain: 35,
                NextPlay: NextPlayKind.FirstDown,
                DriveStartingFieldPosition: 25,
                DriveStartingPeriodNumber: 1,
                DriveStartingSecondsLeftInPeriod: 900,
                DriveResult: null,
                LastPlayDescriptionTemplate: "",
                AwayScoredThisPlay: false,
                HomeScoredThisPlay: false,
                PossessionOnPlay: PossessionOnPlay.None,
                TeamCallingTimeout: teamCallingTimeout
            );
        }

        private PlayEnvironment CreateMockPlayEnvironment(IRandom random, Team awayTeam, Team homeTeam)
        {
            return new PlayEnvironment
            {
                DecisionParameters = new()
                {
                    Random = random,
                    AwayTeam = awayTeam,
                    HomeTeam = homeTeam,
                    AwayTeamActualStrengths = null!,
                    HomeTeamActualStrengths = null!,
                    HomeTeamEstimateOfAway = null!,
                    HomeTeamEstimateOfHome = null!,
                    AwayTeamEstimateOfAway = null!,
                    AwayTeamEstimateOfHome = null!
                },
                PhysicsParams = _physicsParams,
                EventBus = Mock.Of<IEventBus>()
            };
        }

        [Fact]
        public void Run_ShouldTransitionToPostPlayCheckState()
        {
            // Arrange
            var playContext = CreateTestPlayContext();
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(It.IsAny<double>(), It.IsAny<double>()))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            Assert.Equal(GameState.PostPlayCheck, result.NextState);
        }

        [Fact]
        public void Run_ShouldAdjustClockByPlayDurationAndTimeBetweenPlays()
        {
            // Arrange
            var playContext = CreateTestPlayContext(secondsLeftInPeriod: 900, clockRunning: true);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            // Expected: 900 - 6 (play duration) - 35 (standard time between plays) = 859
            Assert.Equal(859, updatedPlayContext.SecondsLeftInPeriod);
        }

        [Fact]
        public void Run_WhenClockStopped_ShouldNotAddTimeBetweenPlays()
        {
            // Arrange
            var playContext = CreateTestPlayContext(secondsLeftInPeriod: 900, clockRunning: false);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            // Expected: 900 - 6 (play duration only, no time between plays) = 894
            Assert.Equal(894, updatedPlayContext.SecondsLeftInPeriod);
        }

        [Fact]
        public void Run_WhenAwayTeamCallsTimeout_ShouldReduceAwayTimeouts()
        {
            // Arrange
            var playContext = CreateTestPlayContext(
                secondsLeftInPeriod: 900,
                awayTimeouts: 3,
                teamCallingTimeout: GameTeam.Away);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(2, updatedPlayContext.AwayTimeoutsRemaining);
            Assert.Equal(3, updatedPlayContext.HomeTimeoutsRemaining); // Should remain unchanged
        }

        [Fact]
        public void Run_WhenHomeTeamCallsTimeout_ShouldReduceHomeTimeouts()
        {
            // Arrange
            var playContext = CreateTestPlayContext(
                secondsLeftInPeriod: 900,
                homeTimeouts: 3,
                teamCallingTimeout: GameTeam.Home);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(2, updatedPlayContext.HomeTimeoutsRemaining);
            Assert.Equal(3, updatedPlayContext.AwayTimeoutsRemaining); // Should remain unchanged
        }

        [Fact]
        public void Run_WhenTimeoutCalled_ShouldNotAddTimeBetweenPlays()
        {
            // Arrange
            var playContext = CreateTestPlayContext(
                secondsLeftInPeriod: 900,
                teamCallingTimeout: GameTeam.Home);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0); // Play duration

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            // Expected: 900 - 6 (play duration only, no time between plays due to timeout) = 894
            Assert.Equal(894, updatedPlayContext.SecondsLeftInPeriod);
        }

        [Fact]
        public void Run_ShouldClampTimeToZeroWhenNegative()
        {
            // Arrange
            var playContext = CreateTestPlayContext(secondsLeftInPeriod: 30);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(50.0); // Long play duration that would make time negative

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(0, updatedPlayContext.SecondsLeftInPeriod);
        }

        [Fact]
        public void Run_ShouldPreserveOtherPlayContextProperties()
        {
            // Arrange
            var originalPlayContext = CreateTestPlayContext(
                secondsLeftInPeriod: 900,
                homeTimeouts: 2,
                awayTimeouts: 1);
            var gameContext = CreateTestGameContext(originalPlayContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0);

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(originalPlayContext.PeriodNumber, updatedPlayContext.PeriodNumber);
            Assert.Equal(originalPlayContext.AwayScore, updatedPlayContext.AwayScore);
            Assert.Equal(originalPlayContext.HomeScore, updatedPlayContext.HomeScore);
            Assert.Equal(originalPlayContext.TeamWithPossession, updatedPlayContext.TeamWithPossession);
            Assert.Equal(originalPlayContext.LineOfScrimmage, updatedPlayContext.LineOfScrimmage);
        }

        [Theory]
        [InlineData(5.0)] // Short play
        [InlineData(8.0)] // Average play  
        [InlineData(12.0)] // Long play
        public void Run_ShouldUseRandomlyGeneratedPlayDuration(double playDuration)
        {
            // Arrange
            var playContext = CreateTestPlayContext(secondsLeftInPeriod: 50);
            var gameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(playDuration);

            // Act
            var result = AdjustClockStep.Run(gameContext);

            // Assert
            var updatedPlayContext = result.Environment.CurrentPlayContext!;
            var expectedTime = 50 - playDuration - 35; // 35 is standard time between plays
            var clampedTime = Math.Clamp(expectedTime, 0, 900); // Constants.SecondsPerQuarter
            Assert.Equal((int)clampedTime, updatedPlayContext.SecondsLeftInPeriod);
        }

        [Fact]
        public void Run_ShouldCallRandomSampleNormalDistributionWithCorrectParameters()
        {
            // Arrange
            var playContext = CreateTestPlayContext();
            var gameContext = CreateTestGameContext(playContext);

            // Act
            AdjustClockStep.Run(gameContext);

            // Assert
            _mockRandom.Verify(r => r.SampleNormalDistribution(6.0, 2.0), Times.Once);
        }

        [Fact]
        public void Run_ShouldIncrementGameContextVersionAndOtherProperties()
        {
            // Arrange
            var playContext = CreateTestPlayContext();
            var originalGameContext = CreateTestGameContext(playContext);
            
            _mockRandom.Setup(r => r.SampleNormalDistribution(6.0, 2.0))
                .Returns(6.0);

            // Act
            var result = AdjustClockStep.Run(originalGameContext);

            // Assert
            Assert.Equal(originalGameContext.Version + 1, result.Version);
            Assert.Equal(originalGameContext.TeamWithPossession, result.TeamWithPossession);
            Assert.Equal(originalGameContext.PlayCountOnDrive, result.PlayCountOnDrive);
            Assert.Same(originalGameContext.Environment.RandomFactory, result.Environment.RandomFactory);
        }
    }
}