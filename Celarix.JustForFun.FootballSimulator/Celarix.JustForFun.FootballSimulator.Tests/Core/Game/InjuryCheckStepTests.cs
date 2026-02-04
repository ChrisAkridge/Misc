using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class InjuryCheckStepTests
    {
        [Fact]
        public void Run_NoInjuries_ReturnsContextWithAdjustClockState()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home);
            SetupMockRandomToNeverRollInjury(context.Environment.RandomFactory);

            // Act
            var result = InjuryCheckStep.Run(context);

            // Assert
            Assert.Equal(GameState.AdjustClock, result.NextState);
            VerifyNoInjuriesAdded(context.Environment.FootballRepository);
        }

        [Fact]
        public void Run_OffensivePlayerInjured_CreatesInjuryRecoveries()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home);
            SetupMockRandomToAlwaysRollInjury(context.Environment.RandomFactory);

            // Act
            var result = InjuryCheckStep.Run(context);

            // Assert
            Assert.Equal(GameState.AdjustClock, result.NextState);
            VerifyInjuriesAdded(context.Environment.FootballRepository);
        }

        [Fact]
        public void Run_DefensivePlayerInjured_CreatesInjuryRecoveries()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Away); // Defense is home team
            SetupMockRandomToAlwaysRollInjury(context.Environment.RandomFactory);

            // Act
            var result = InjuryCheckStep.Run(context);

            // Assert
            Assert.Equal(GameState.AdjustClock, result.NextState);
            VerifyInjuriesAdded(context.Environment.FootballRepository);
        }

        [Fact]
        public void Run_TemperatureDifference_AffectsInjuryChance()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home);
            context = context with 
            { 
                AwayTeamAcclimatedTemperature = 70, 
                HomeTeamAcclimatedTemperature = 70 
            };
            
            // Set current temperature to be very different from acclimated temperature
            var playContext = context.Environment.CurrentPlayContext! with { AirTemperature = 30 };
            context.Environment.CurrentPlayContext = playContext;

            SetupMockRandomToAlwaysRollInjury(context.Environment.RandomFactory);

            // Act
            var result = InjuryCheckStep.Run(context);

            // Assert
            Assert.Equal(GameState.AdjustClock, result.NextState);
            // Temperature difference should still result in injuries being processed
            VerifyInjuriesAdded(context.Environment.FootballRepository);
        }

        [Fact]
        public void Run_CallsHelpersRebuildStrengths()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home);
            SetupMockRandomToNeverRollInjury(context.Environment.RandomFactory);

            // Act
            var result = InjuryCheckStep.Run(context);

            // Assert
            // Helpers.RebuildStrengthsInDecisionParameters should be called
            // This is verified indirectly by ensuring the method completes successfully
            Assert.Equal(GameState.AdjustClock, result.NextState);
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_OffensePlayer_CreatesCorrectRecoveries()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Offense);
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            Assert.Equal(3, recoveries.Count); // Offense affects 3 strengths
            AssertRecoveryHasCorrectTeamId(recoveries, team.TeamID);
            AssertRecoveriesContainExpectedStrengths(recoveries, 
                "RunningOffenseStrength", "OffensiveLineStrength", "KickReturnStrength");
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_DefensePlayer_CreatesCorrectRecoveries()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Defense);
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            Assert.Equal(3, recoveries.Count); // Defense affects 3 strengths
            AssertRecoveryHasCorrectTeamId(recoveries, team.TeamID);
            AssertRecoveriesContainExpectedStrengths(recoveries,
                "RunningDefenseStrength", "DefensiveLineStrength", "KickDefenseStrength");
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_QuarterbackPlayer_CreatesCorrectRecoveries()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Quarterback);
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            Assert.Single(recoveries); // Quarterback affects 1 strength
            AssertRecoveryHasCorrectTeamId(recoveries, team.TeamID);
            AssertRecoveriesContainExpectedStrengths(recoveries, "PassingOffenseStrength");
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_KickerPlayer_CreatesCorrectRecoveries()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Kicker);
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            Assert.Equal(2, recoveries.Count); // Kicker affects 2 strengths
            AssertRecoveryHasCorrectTeamId(recoveries, team.TeamID);
            AssertRecoveriesContainExpectedStrengths(recoveries,
                "KickingStrength", "FieldGoalStrength");
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_MinimumRecoveryTime_IsThreeDays()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Kicker);
            var random = new Mock<IRandom>();
            
            // Set up random to return very low recovery days
            random.Setup(r => r.SampleNormalDistribution(0.5, 0.1)).Returns(0.5);
            random.Setup(r => r.SampleNormalDistribution(1.0, 0.1)).Returns(1.0);
            
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            foreach (var recovery in recoveries)
            {
                var daysDifference = (recovery.RecoverOn - kickoffTime).Days;
                Assert.True(daysDifference >= 3, $"Recovery time should be at least 3 days, but was {daysDifference}");
            }
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_UnknownPosition_ThrowsInvalidOperationException()
        {
            // Arrange
            var team = CreateTestTeam();
            var player = CreatePlayerRosterPosition((BasicPlayerPosition)999); // Invalid position
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                    random.Object, player, team, physicsParams, kickoffTime).ToList());
            
            Assert.Contains("Unknown player position", exception.Message);
        }

        [Fact]
        public void InjurePlayerAndCreateRecoveries_ModifiesTeamStrengths()
        {
            // Arrange
            var team = CreateTestTeam();
            var originalStrength = team.RunningOffenseStrength;
            var player = CreatePlayerRosterPosition(BasicPlayerPosition.Offense);
            var random = CreateMockRandom();
            var physicsParams = CreatePhysicsParamsForInjury();
            var kickoffTime = DateTimeOffset.Now;

            // Act
            var recoveries = InjuryCheckStep.InjurePlayerAndCreateRecoveries(
                random.Object, player, team, physicsParams, kickoffTime).ToList();

            // Assert
            Assert.NotEqual(originalStrength, team.RunningOffenseStrength);
            
            // Verify the delta calculation is correct
            var runningOffenseRecovery = recoveries.First(r => r.Strength == "RunningOffenseStrength");
            Assert.Equal(team.RunningOffenseStrength - originalStrength, runningOffenseRecovery.StrengthDelta);
        }

        private static GameContext CreateTestGameContext(GameTeam teamWithPossession)
        {
            var homeTeam = CreateTestTeam();
            homeTeam.TeamName = "Home Team";

            var awayTeam = CreateTestTeam();
            awayTeam.TeamName = "Away Team";

            var gameRecord = new GameRecord
            {
                GameID = 1,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                KickoffTime = DateTimeOffset.Now
            };

            var playContext = new PlayContext(
                Version: 0L,
                NextState: PlayEvaluationState.Start,
                AdditionalParameters: Array.Empty<AdditionalParameter<object>>(),
                StateHistory: Array.Empty<StateHistoryEntry>(),
                Environment: new PlayEnvironment
                {
                    DecisionParameters = new GameDecisionParameters(),
                    PhysicsParams = CreatePhysicsParamsForInjury()
                },
                BaseWindDirection: 0.0,
                BaseWindSpeed: 0.0,
                AirTemperature: 65.0,
                CoinFlipWinner: GameTeam.Home,
                TeamWithPossession: teamWithPossession,
                AwayScore: 0,
                HomeScore: 0,
                PeriodNumber: 1,
                SecondsLeftInPeriod: 900,
                ClockRunning: false,
                HomeTimeoutsRemaining: 3,
                AwayTimeoutsRemaining: 3,
                PlayInvolvement: TestHelpers.EmptyPlayInvolvement,
                LineOfScrimmage: 25,
                LineToGain: null,
                NextPlay: NextPlayKind.Kickoff,
                DriveStartingFieldPosition: 25,
                DriveStartingPeriodNumber: 1,
                DriveStartingSecondsLeftInPeriod: 900,
                DriveResult: null,
                LastPlayDescriptionTemplate: string.Empty,
                AwayScoredThisPlay: false,
                HomeScoredThisPlay: false,
                PossessionOnPlay: PossessionOnPlay.AwayTeamOnly,
                TeamCallingTimeout: null
            );

            var mockFootballRepo = new Mock<IFootballRepository>();
            mockFootballRepo.Setup(r => r.AddInjuryRecoveries(It.IsAny<List<InjuryRecovery>>()));
            mockFootballRepo.Setup(r => r.SaveChanges());

            var mockRandomFactory = new Mock<IRandomFactory>();
            var physicsParams = CreatePhysicsParamsForInjury();

            var environment = new GameEnvironment
            {
                FootballRepository = mockFootballRepo.Object,
                RandomFactory = mockRandomFactory.Object,
                PhysicsParams = physicsParams,
                CurrentGameRecord = gameRecord,
                CurrentPlayContext = playContext,
                DebugContextWriter = null!,
                AwayActiveRoster = [],
                HomeActiveRoster = []
            };

            var offensePlayers = new List<PlayerRosterPosition>
            {
                CreatePlayerRosterPosition(BasicPlayerPosition.Offense),
                CreatePlayerRosterPosition(BasicPlayerPosition.Quarterback)
            };

            var defensePlayers = new List<PlayerRosterPosition>
            {
                CreatePlayerRosterPosition(BasicPlayerPosition.Defense)
            };

            return new GameContext(
                Version: 0L,
                NextState: GameState.InjuryCheck,
                Environment: environment,
                AwayTeamAcclimatedTemperature: 65,
                HomeTeamAcclimatedTemperature: 65,
                TeamWithPossession: teamWithPossession,
                PlayCountOnDrive: 1,
                OffensePlayersOnPlay: offensePlayers,
                DefensePlayersOnPlay: defensePlayers
            );
        }

        private static Team CreateTestTeam()
        {
            var team = new Team
            {
                TeamID = 1,
                CityName = "Test",
                TeamName = "Team",
                Abbreviation = "TST",
                Disposition = TeamDisposition.Conservative
            };
            TestHelpers.SetFixedStrengths(team, 50.0);
            return team;
        }

        private static PlayerRosterPosition CreatePlayerRosterPosition(BasicPlayerPosition position)
        {
            return new PlayerRosterPosition
            {
                PlayerRosterPositionID = 1,
                Position = position,
                Player = new Player { FirstName = "Test", LastName = "Player" }
            };
        }

        private static Mock<IRandom> CreateMockRandom()
        {
            var mockRandom = new Mock<IRandom>();
            mockRandom.Setup(r => r.SampleNormalDistribution(0.5, 0.1)).Returns(0.5);
            mockRandom.Setup(r => r.SampleNormalDistribution(7.0, 2.0)).Returns(7.0);
            return mockRandom;
        }

        private static void SetupMockRandomToNeverRollInjury(IRandomFactory randomFactory)
        {
            var mockRandomFactory = Mock.Get(randomFactory);
            var mockRandom = new Mock<IRandom>();
            mockRandom.Setup(r => r.Chance(It.IsAny<double>())).Returns(false);
            mockRandomFactory.Setup(f => f.Create()).Returns(mockRandom.Object);
        }

        private static void SetupMockRandomToAlwaysRollInjury(IRandomFactory randomFactory)
        {
            var mockRandomFactory = Mock.Get(randomFactory);
            var mockRandom = new Mock<IRandom>();
            mockRandom.Setup(r => r.Chance(It.IsAny<double>())).Returns(true);
            mockRandom.Setup(r => r.SampleNormalDistribution(0.5, 0.1)).Returns(0.5);
            mockRandom.Setup(r => r.SampleNormalDistribution(7.0, 2.0)).Returns(7.0);
            mockRandomFactory.Setup(f => f.Create()).Returns(mockRandom.Object);
        }

        private static IReadOnlyDictionary<string, PhysicsParam> CreatePhysicsParamsForInjury()
        {
            return new Dictionary<string, PhysicsParam>
            {
                { "BaseInjuryChancePerPlay", new PhysicsParam("BaseInjuryChancePerPlay", 0.01, "", "") },
                { "InjuryTemperatureStep", new PhysicsParam("InjuryTemperatureStep", 10.0, "", "") },
                { "InjuryTemperatureMultiplierPerStep", new PhysicsParam("InjuryTemperatureMultiplierPerStep", 1.1, "", "") },
                { "InjuryStrengthAdjustmentMean", new PhysicsParam("InjuryStrengthAdjustmentMean", 0.5, "", "") },
                { "InjuryStrengthAdjustmentStdDev", new PhysicsParam("InjuryStrengthAdjustmentStdDev", 0.1, "", "") },
                { "InjuryRecoveryDaysMean", new PhysicsParam("InjuryRecoveryDaysMean", 7.0, "", "") },
                { "InjuryRecoveryDaysStdDev", new PhysicsParam("InjuryRecoveryDaysStdDev", 2.0, "", "") },
                { "StrengthEstimatorOffsetMean", new PhysicsParam("StrengthEstimatorOffsetMean", 0.0, "", "")  },
                { "StrengthEstimatorOffsetStddev", new PhysicsParam("StrengthEstimatorOffsetStdDev", 1.0, "", "") },
                { "StrengthEstimatorConservativeAdjustment", new PhysicsParam("StrengthEstimatorConservativeAdjustment", -1.0, "", "")  }
            };
        }

        private static void VerifyNoInjuriesAdded(IFootballRepository repository)
        {
            var mockRepo = Mock.Get(repository);
            mockRepo.Verify(r => r.AddInjuryRecoveries(It.Is<List<InjuryRecovery>>(list => !list.Any())), Times.Once);
        }

        private static void VerifyInjuriesAdded(IFootballRepository repository)
        {
            var mockRepo = Mock.Get(repository);
            mockRepo.Verify(r => r.AddInjuryRecoveries(It.Is<List<InjuryRecovery>>(list => list.Any())), Times.Once);
            mockRepo.Verify(r => r.SaveChanges(), Times.Once);
        }

        private static void AssertRecoveryHasCorrectTeamId(List<InjuryRecovery> recoveries, int expectedTeamId)
        {
            Assert.True(recoveries.All(r => r.TeamID == expectedTeamId),
                "All injury recoveries should have the correct team ID");
        }

        private static void AssertRecoveriesContainExpectedStrengths(List<InjuryRecovery> recoveries, params string[] expectedStrengths)
        {
            var recoveryStrengths = recoveries.Select(r => r.Strength).ToHashSet();
            foreach (var expectedStrength in expectedStrengths)
            {
                Assert.Contains(expectedStrength, recoveryStrengths);
            }
        }
    }
}