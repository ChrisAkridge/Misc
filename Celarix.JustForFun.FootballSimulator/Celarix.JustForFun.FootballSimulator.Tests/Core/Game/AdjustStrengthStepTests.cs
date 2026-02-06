using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class AdjustStrengthStepTests
    {
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

        private static GameContext CreateGameContextWithTeams(GameTeam teamWithPossession, PlayInvolvement playInvolvement)
        {
            var homeTeam = CreateTestTeam();
            homeTeam.TeamName = "Home Team";
            
            var awayTeam = CreateTestTeam();
            awayTeam.TeamName = "Away Team";

            var mockRandomFactory = new Mock<IRandomFactory>();
            var mockRandom = new Mock<IRandom>();
            mockRandom.Setup(r => r.SampleNormalDistribution(It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(1.1); // 10% increase multiplier

            mockRandomFactory.Setup(f => f.Create()).Returns(mockRandom.Object);

            var physicsParams = new Dictionary<string, PhysicsParam>
            {
                { "StrengthAdjustmentMultiplierMean", new PhysicsParam("StrengthAdjustmentMultiplierMean", 1.0, "", "") },
                { "StrengthAdjustmentMultiplierStddev", new PhysicsParam("StrengthAdjustmentMultiplierStddev", 0.1, "", "") },
                { "StrengthEstimatorOffsetMean", new PhysicsParam("StrengthEstimatorOffsetMean", 0.0, "offset", "offset") },
                { "StrengthEstimatorOffsetStddev", new PhysicsParam("StrengthEstimatorOffsetStddev", 0.0, "offset", "offset") },
                { "StrengthEstimatorConservativeAdjustment", new PhysicsParam("StrengthEstimatorConservativeAdjustment", 0.0, "offset", "offset") }
            };

            return TestHelpers.EmptyGameContext with
            {
                Environment = new GameEnvironment
                {
                    CurrentGameRecord = new GameRecord
                    {
                        HomeTeam = homeTeam,
                        AwayTeam = awayTeam
                    },
                    CurrentPlayContext = TestHelpers.EmptyPlayContext with
                    {
                        TeamWithPossession = teamWithPossession,
                        PlayInvolvement = playInvolvement,
                        Environment = new PlayEnvironment
                        {
                            PhysicsParams = physicsParams,
                            DecisionParameters = new GameDecisionParameters
                            {
                                AwayTeam = awayTeam,
                                HomeTeam = homeTeam,
                                AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(awayTeam, GameTeam.Away),
                                HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(homeTeam, GameTeam.Home),
                                Random = null!,
                                HomeTeamEstimateOfAway = null!,
                                HomeTeamEstimateOfHome = null!,
                                AwayTeamEstimateOfAway = null!,
                                AwayTeamEstimateOfHome = null!
                            },
                            EventBus = Mock.Of<IEventBus>()
                        }
                    },
                    RandomFactory = mockRandomFactory.Object,
                    PhysicsParams = physicsParams,
                    FootballRepository = null!,
                    AwayActiveRoster = [],
                    HomeActiveRoster = [],
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
        }

        [Fact]
        public void Run_HomeTeamOffense_AdjustsCorrectStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Home, playInvolvement);
            var originalHomeOffensiveLineStrength = context.Environment.CurrentGameRecord!.HomeTeam!.OffensiveLineStrength;
            var originalAwayDefensiveLineStrength = context.Environment.CurrentGameRecord!.AwayTeam!.DefensiveLineStrength;

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            Assert.Equal(GameState.DeterminePlayersOnPlay, result.NextState);
            
            // Home team (offense) should have adjusted offensive and running offense strengths
            Assert.NotEqual(originalHomeOffensiveLineStrength, context.Environment.CurrentGameRecord.HomeTeam.OffensiveLineStrength);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.OffensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.RunningOffenseStrength, 1);
            
            // Away team (defense) should have adjusted defensive and running defense strengths  
            Assert.NotEqual(originalAwayDefensiveLineStrength, context.Environment.CurrentGameRecord.AwayTeam.DefensiveLineStrength);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam.DefensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam.RunningDefenseStrength, 1);
        }

        [Fact]
        public void Run_AwayTeamOffense_AdjustsCorrectStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: true,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Away, playInvolvement);

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            Assert.Equal(GameState.DeterminePlayersOnPlay, result.NextState);
            
            // Away team (offense) should have adjusted offensive and passing offense strengths
            Assert.Equal(55.0, context.Environment.CurrentGameRecord!.AwayTeam!.OffensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam.PassingOffenseStrength, 1);
            
            // Home team (defense) should have adjusted defensive and passing defense strengths  
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam!.DefensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.PassingDefenseStrength, 1);
        }

        [Fact]
        public void Run_KickPlay_AdjustsKickingStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: true,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Home, playInvolvement);

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            // Home team (offense) should have adjusted kicking and field goal strengths
            Assert.Equal(55.0, context.Environment.CurrentGameRecord!.HomeTeam!.KickingStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.FieldGoalStrength, 1);
        }

        [Fact]
        public void Run_KickWithDefenseRun_AdjustsKickReturnStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: true,
                InvolvesDefenseRun: true,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Home, playInvolvement);

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            // Away team (defense) should have adjusted kick return strength
            Assert.Equal(55.0, context.Environment.CurrentGameRecord!.AwayTeam!.KickReturnStrength, 1);
            
            // Home team (offense) should have adjusted kick defense strength
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam!.KickDefenseStrength, 1);
        }

        [Fact]
        public void Run_DefenseRunWithoutKick_AdjustsRunningStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: true,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Home, playInvolvement);

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            // Away team (defense) should have adjusted running offense strength (for return)
            Assert.Equal(55.0, context.Environment.CurrentGameRecord!.AwayTeam!.RunningOffenseStrength, 1);
            
            // Home team (offense) should have adjusted running defense strength
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam!.RunningDefenseStrength, 1);
        }

        [Fact]
        public void AdjustStrength_AppliesMultiplierCorrectly()
        {
            // Arrange
            var team = CreateTestTeam();
            var mockRandom = new Mock<IRandom>();
            mockRandom.Setup(r => r.SampleNormalDistribution(1.0, 0.1)).Returns(1.2);

            var originalStrength = team.RunningOffenseStrength;

            // Act
            AdjustStrengthStep.AdjustStrength(team, mockRandom.Object, nameof(Team.RunningOffenseStrength), 1.0, 0.1);

            // Assert
            var expectedNewStrength = originalStrength * 1.2;
            Assert.Equal(expectedNewStrength, team.RunningOffenseStrength, 5);
            mockRandom.Verify(r => r.SampleNormalDistribution(1.0, 0.1), Times.Once);
        }

        [Theory]
        [InlineData(nameof(Team.RunningOffenseStrength), 75.0)]
        [InlineData(nameof(Team.RunningDefenseStrength), 75.0)]
        [InlineData(nameof(Team.PassingOffenseStrength), 75.0)]
        [InlineData(nameof(Team.PassingDefenseStrength), 75.0)]
        [InlineData(nameof(Team.OffensiveLineStrength), 75.0)]
        [InlineData(nameof(Team.DefensiveLineStrength), 75.0)]
        [InlineData(nameof(Team.KickingStrength), 75.0)]
        [InlineData(nameof(Team.FieldGoalStrength), 75.0)]
        [InlineData(nameof(Team.KickReturnStrength), 75.0)]
        [InlineData(nameof(Team.KickDefenseStrength), 75.0)]
        [InlineData(nameof(Team.ClockManagementStrength), 75.0)]
        public void GetStrength_ReturnsCorrectValue(string strengthPropertyName, double expectedValue)
        {
            // Arrange
            var team = CreateTestTeam();
            TestHelpers.SetFixedStrengths(team, expectedValue);

            // Act
            var result = AdjustStrengthStep.GetStrength(team, strengthPropertyName);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        [Fact]
        public void GetStrength_ThrowsForInvalidPropertyName()
        {
            // Arrange
            var team = CreateTestTeam();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                AdjustStrengthStep.GetStrength(team, "InvalidPropertyName"));
            
            Assert.Contains("Invalid strength property name: InvalidPropertyName", exception.Message);
        }

        [Theory]
        [InlineData(nameof(Team.RunningOffenseStrength), 85.0)]
        [InlineData(nameof(Team.RunningDefenseStrength), 85.0)]
        [InlineData(nameof(Team.PassingOffenseStrength), 85.0)]
        [InlineData(nameof(Team.PassingDefenseStrength), 85.0)]
        [InlineData(nameof(Team.OffensiveLineStrength), 85.0)]
        [InlineData(nameof(Team.DefensiveLineStrength), 85.0)]
        [InlineData(nameof(Team.KickingStrength), 85.0)]
        [InlineData(nameof(Team.FieldGoalStrength), 85.0)]
        [InlineData(nameof(Team.KickReturnStrength), 85.0)]
        [InlineData(nameof(Team.KickDefenseStrength), 85.0)]
        [InlineData(nameof(Team.ClockManagementStrength), 85.0)]
        public void SetStrength_SetsCorrectValue(string strengthPropertyName, double newValue)
        {
            // Arrange
            var team = CreateTestTeam();

            // Act
            AdjustStrengthStep.SetStrength(team, strengthPropertyName, newValue);

            // Assert
            var retrievedValue = AdjustStrengthStep.GetStrength(team, strengthPropertyName);
            Assert.Equal(newValue, retrievedValue);
        }

        [Fact]
        public void SetStrength_ThrowsForInvalidPropertyName()
        {
            // Arrange
            var team = CreateTestTeam();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                AdjustStrengthStep.SetStrength(team, "InvalidPropertyName", 100.0));
            
            Assert.Contains("Invalid strength property name: InvalidPropertyName", exception.Message);
        }

        [Fact]
        public void Run_CombinedPlayInvolvement_AdjustsAllRelevantStrengths()
        {
            // Arrange
            var playInvolvement = new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: true,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 11,
                DefensivePlayersInvolved: 11
            );

            var context = CreateGameContextWithTeams(GameTeam.Home, playInvolvement);

            // Act
            var result = AdjustStrengthStep.Run(context);

            // Assert
            // Home team (offense) should have all offense-related strengths adjusted
            Assert.Equal(55.0, context.Environment.CurrentGameRecord!.HomeTeam!.OffensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.RunningOffenseStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.HomeTeam.PassingOffenseStrength, 1);
            
            // Away team (defense) should have all defense-related strengths adjusted
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam!.DefensiveLineStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam.RunningDefenseStrength, 1);
            Assert.Equal(55.0, context.Environment.CurrentGameRecord.AwayTeam.PassingDefenseStrength, 1);
        }
    }
}