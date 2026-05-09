using Celarix.JustForFun.FootballSimulator.Core;
using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class StartNextPeriodStepTests
    {
        private readonly Mock<IFootballRepository> _mockRepository;
        private readonly Mock<IRandomFactory> _mockRandomFactory;
        private readonly Mock<IRandom> _mockRandom;
        private readonly GameEnvironment _environment;

        public StartNextPeriodStepTests()
        {
            _mockRepository = new Mock<IFootballRepository>();
            _mockRandomFactory = new Mock<IRandomFactory>();
            _mockRandom = new Mock<IRandom>();
            _mockRandomFactory.Setup(f => f.Create()).Returns(_mockRandom.Object);

            _environment = new GameEnvironment
            {
                FootballRepository = _mockRepository.Object,
                PhysicsParams = TestHelpers.EmptyPhysicsParams,
                DebugContextWriter = null!,
                RandomFactory = _mockRandomFactory.Object,
                CurrentGameRecord = new GameRecord
                {
                    GameID = 1
                },
                AwayActiveRoster = [],
                HomeActiveRoster = [],
                CurrentPlayContext = TestHelpers.EmptyPlayContext,
                EventBus = Mock.Of<IEventBus>()
            };
        }

        [Fact]
        public void Run_ShouldHandleCoinTossNeeded_WhenPeriodRequiresCoinToss()
        {
            // Arrange - Period 1 requires coin toss (period % 4 == 1)
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 0 // NextQuarterActions will make this period 1
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            _mockRandom.Setup(r => r.NextDouble()).Returns(0.3); // Away receives

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(1, resultPlayContext.PeriodNumber);
            Assert.Equal(Constants.SecondsPerQuarter, resultPlayContext.SecondsLeftInPeriod);
            Assert.Equal(GameTeam.Away, resultPlayContext.CoinFlipWinner);
            Assert.Equal(GameTeam.Home, resultPlayContext.TeamWithPossession); // Kicking team is opposite
            Assert.Equal(NextPlayKind.Kickoff, resultPlayContext.NextPlay);
            Assert.Equal(35, resultPlayContext.TeamYardToInternalYard(GameTeam.Home, 35));
            Assert.Null(resultPlayContext.LineToGain);
            Assert.Equal(3, resultPlayContext.AwayTimeoutsRemaining);
            Assert.Equal(3, resultPlayContext.HomeTimeoutsRemaining);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_ShouldHandleCoinTossNeeded_WhenHomeReceives()
        {
            // Arrange
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 0 // NextQuarterActions will make this period 1
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            _mockRandom.Setup(r => r.NextDouble()).Returns(0.7); // Home receives

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(GameTeam.Home, resultPlayContext.CoinFlipWinner);
            Assert.Equal(GameTeam.Away, resultPlayContext.TeamWithPossession); // Kicking team is opposite
        }

        [Fact]
        public void Run_ShouldHandleCoinTossLoserReceivesPossession_WhenPeriodIsThird()
        {
            // Arrange - Period 3 means coin toss loser receives (period % 4 == 3)
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 2, // NextQuarterActions will make this period 3
                CoinFlipWinner = GameTeam.Away
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(3, resultPlayContext.PeriodNumber);
            Assert.Equal(NextPlayKind.Kickoff, resultPlayContext.NextPlay);
            Assert.Equal(GameTeam.Away, resultPlayContext.TeamWithPossession); // Same as coin flip winner
            Assert.Equal(65, resultPlayContext.TeamYardToInternalYard(GameTeam.Away, 35));
            Assert.Null(resultPlayContext.LineToGain);
        }

        [Fact]
        public void Run_ShouldHandleOvertimePeriod_WhenPeriodNumberIsGreaterThanFour()
        {
            // Arrange - Period 5 is overtime
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 4 // NextQuarterActions will make this period 5
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            _mockRandom.Setup(r => r.NextDouble()).Returns(0.4); // Away receives

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(5, resultPlayContext.PeriodNumber);
            Assert.Equal(Constants.SecondsPerOvertimePeriod, resultPlayContext.SecondsLeftInPeriod);
            Assert.Equal(2, resultPlayContext.AwayTimeoutsRemaining); // Overtime timeouts
            Assert.Equal(2, resultPlayContext.HomeTimeoutsRemaining);
        }

        [Fact]
        public void Run_ShouldHandleRegularPeriod_WhenNoCoinTossOrSpecialLogicNeeded()
        {
            // Arrange - Period 2 or 4 (no special coin toss logic)
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 1, // NextQuarterActions will make this period 2
                TeamWithPossession = GameTeam.Home,
                LineOfScrimmage = 50
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(2, resultPlayContext.PeriodNumber);
            Assert.Equal(Constants.SecondsPerQuarter, resultPlayContext.SecondsLeftInPeriod);
            Assert.Equal(GameTeam.Home, resultPlayContext.TeamWithPossession); // Preserved
            Assert.Equal(50, resultPlayContext.LineOfScrimmage); // Preserved
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_ShouldSaveQuarterBoxScoresAndRepository()
        {
            // Arrange
            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };

            // Act
            StartNextPeriodStep.Run(context);

            // Assert
            _mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_ShouldIncrementPeriodNumberCorrectly()
        {
            // Arrange
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 3
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            Assert.Equal(4, result.Environment.CurrentPlayContext!.PeriodNumber);
        }

        [Fact]
        public void Run_ShouldHandleSecondOvertimePeriod()
        {
            // Arrange - Period 6 is second overtime
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 5, // NextQuarterActions will make this period 6
                CoinFlipWinner = GameTeam.Home
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(6, resultPlayContext.PeriodNumber);
            Assert.Equal(Constants.SecondsPerOvertimePeriod, resultPlayContext.SecondsLeftInPeriod);
            // Period 6 % 4 == 2, so no special coin toss logic
            Assert.Equal(GameTeam.Home, resultPlayContext.CoinFlipWinner); // Preserved
        }

        [Theory]
        [InlineData(0, 1, true, false)] // Period 1: coin toss needed
        [InlineData(1, 2, false, false)] // Period 2: no special logic
        [InlineData(2, 3, false, true)] // Period 3: coin toss loser receives
        [InlineData(3, 4, false, false)] // Period 4: no special logic
        [InlineData(4, 5, true, false)] // Period 5: coin toss needed (overtime)
        [InlineData(7, 8, false, true)] // Period 8: coin toss loser receives
        public void Run_ShouldHandleCorrectLogicBasedOnPeriodNumber(
            int currentPeriod, 
            int expectedNextPeriod, 
            bool expectCoinToss, 
            bool expectLoserReceives)
        {
            // Arrange
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = currentPeriod,
                CoinFlipWinner = GameTeam.Away
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            if (expectCoinToss)
            {
                _mockRandom.Setup(r => r.NextDouble()).Returns(0.6); // Home receives
            }

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(expectedNextPeriod, resultPlayContext.PeriodNumber);

            if (expectCoinToss)
            {
                Assert.Equal(NextPlayKind.Kickoff, resultPlayContext.NextPlay);
            }
            else if (expectLoserReceives)
            {
                Assert.Equal(NextPlayKind.Kickoff, resultPlayContext.NextPlay);
                Assert.Equal(GameTeam.Away, resultPlayContext.TeamWithPossession);
            }
        }

        [Fact]
        public void Run_ShouldPreserveOtherPlayContextProperties()
        {
            // Arrange
            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = 1,
                AwayScore = 14,
                HomeScore = 7,
                BaseWindDirection = 45.0,
                BaseWindSpeed = 10.0,
                AirTemperature = 72.5
            };

            var context = TestHelpers.EmptyGameContext with
            {
                Environment = _environment
            };
            _environment.CurrentPlayContext = playContext;

            // Act
            var result = StartNextPeriodStep.Run(context);

            // Assert
            var resultPlayContext = result.Environment.CurrentPlayContext!;
            Assert.Equal(14, resultPlayContext.AwayScore);
            Assert.Equal(7, resultPlayContext.HomeScore);
            Assert.Equal(45.0, resultPlayContext.BaseWindDirection);
            Assert.Equal(10.0, resultPlayContext.BaseWindSpeed);
            Assert.Equal(72.5, resultPlayContext.AirTemperature);
        }
    }
}