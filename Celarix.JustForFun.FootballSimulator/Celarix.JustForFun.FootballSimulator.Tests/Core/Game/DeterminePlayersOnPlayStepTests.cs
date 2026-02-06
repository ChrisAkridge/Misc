using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class DeterminePlayersOnPlayStepTests
    {
        [Fact]
        public void Run_HomeTeamOffense_AssignsPlayersCorrectly()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 5,
                DefensivePlayersInvolved: 4
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(GameState.InjuryCheck, result.NextState);
            Assert.Equal(5, result.OffensePlayersOnPlay!.Count);
            Assert.Equal(4, result.DefensePlayersOnPlay!.Count);
        }

        [Fact]
        public void Run_AwayTeamOffense_AssignsPlayersCorrectly()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Away, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 3,
                DefensivePlayersInvolved: 6
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(GameState.InjuryCheck, result.NextState);
            Assert.Equal(3, result.OffensePlayersOnPlay!.Count);
            Assert.Equal(6, result.DefensePlayersOnPlay!.Count);
        }

        [Fact]
        public void Run_WithOffensePass_IncludesQuarterback()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: true,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 5,
                DefensivePlayersInvolved: 4
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(5, result.OffensePlayersOnPlay!.Count);
            Assert.Contains(result.OffensePlayersOnPlay, p => p.Position == BasicPlayerPosition.Quarterback);
        }

        [Fact]
        public void Run_WithKick_IncludesKicker()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: true,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 3,
                DefensivePlayersInvolved: 4
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(3, result.OffensePlayersOnPlay!.Count);
            Assert.Contains(result.OffensePlayersOnPlay, p => p.Position == BasicPlayerPosition.Kicker);
        }

        [Fact]
        public void Run_WithPassAndKick_IncludesBothQuarterbackAndKicker()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: true,
                InvolvesKick: true,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 5,
                DefensivePlayersInvolved: 4
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(5, result.OffensePlayersOnPlay!.Count);
            Assert.Contains(result.OffensePlayersOnPlay, p => p.Position == BasicPlayerPosition.Quarterback);
            Assert.Contains(result.OffensePlayersOnPlay, p => p.Position == BasicPlayerPosition.Kicker);
        }

        [Fact]
        public void Run_ClampsOffensePlayersToMaximum12()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 20, // More than maximum
                DefensivePlayersInvolved: 4
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(12, result.OffensePlayersOnPlay!.Count);
        }

        [Fact]
        public void Run_ClampsDefensePlayersToMaximum11()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 5,
                DefensivePlayersInvolved: 20 // More than maximum
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(11, result.DefensePlayersOnPlay!.Count);
        }

        [Fact]
        public void Run_ClampsPlayersToMinimumZero()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: -5, // Negative value
                DefensivePlayersInvolved: -3  // Negative value
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Empty(result.OffensePlayersOnPlay!);
            Assert.Empty(result.DefensePlayersOnPlay!);
        }

        [Fact]
        public void Run_WithZeroPlayers_HandlesCorrectly()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 0,
                DefensivePlayersInvolved: 0
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Empty(result.OffensePlayersOnPlay!);
            Assert.Empty(result.DefensePlayersOnPlay!);
        }

        [Fact]
        public void Run_WithQuarterbackAndLargePlayerCount_DoesNotExceedRosterSize()
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: false,
                InvolvesOffensePass: true,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 12, // Will be reduced by 1 for QB
                DefensivePlayersInvolved: 4
            ), rosterSize: 15);

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(12, result.OffensePlayersOnPlay!.Count);
            Assert.Contains(result.OffensePlayersOnPlay, p => p.Position == BasicPlayerPosition.Quarterback);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(11, 11)]
        [InlineData(12, 12)]
        public void Run_RespectsOffensivePlayerCount(int requestedPlayers, int expectedPlayers)
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: requestedPlayers,
                DefensivePlayersInvolved: 5
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(expectedPlayers, result.OffensePlayersOnPlay!.Count);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(11, 11)]
        public void Run_RespectsDefensivePlayerCount(int requestedPlayers, int expectedPlayers)
        {
            // Arrange
            var context = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 5,
                DefensivePlayersInvolved: requestedPlayers
            ));

            // Act
            var result = DeterminePlayersOnPlayStep.Run(context);

            // Assert
            Assert.Equal(expectedPlayers, result.DefensePlayersOnPlay!.Count);
        }

        [Fact]
        public void Run_PreservesOriginalContext_ExceptForPlayersAndState()
        {
            // Arrange
            var originalContext = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 3,
                DefensivePlayersInvolved: 3
            ));
            var originalVersion = originalContext.Version;
            var originalTeam = originalContext.TeamWithPossession;

            // Act
            var result = DeterminePlayersOnPlayStep.Run(originalContext);

            // Assert
            Assert.Equal(originalVersion + 1, result.Version);
            Assert.Equal(originalTeam, result.TeamWithPossession);
            Assert.Equal(originalContext.Environment, result.Environment);
            Assert.Equal(originalContext.AwayTeamAcclimatedTemperature, result.AwayTeamAcclimatedTemperature);
            Assert.Equal(originalContext.HomeTeamAcclimatedTemperature, result.HomeTeamAcclimatedTemperature);
        }

        [Fact]
        public void Run_PerformsTemplateSubstitution()
        {
            // Arrange
            var originalContext = CreateTestGameContext(GameTeam.Home, new PlayInvolvement(
                InvolvesOffenseRun: true,
                InvolvesOffensePass: false,
                InvolvesKick: false,
                InvolvesDefenseRun: false,
                OffensivePlayersInvolved: 3,
                DefensivePlayersInvolved: 3
            ));
            originalContext.Environment.CurrentPlayContext = originalContext.Environment.CurrentPlayContext! with
            {
                LineOfScrimmage = 50,
                LastPlayDescriptionTemplate = "Offense {OffAbbr}: {OffPlayer0}, {OffPlayer1}, {OffPlayer2}. Defense {DefAbbr}: {DefPlayer0}, {DefPlayer1}, {DefPlayer2}. LoS is {LoS}."
            };

            // Act
            var result = DeterminePlayersOnPlayStep.Run(originalContext);

            // Assert
            Assert.Equal("Offense HOM: Player 19, Test Quarterback, Test Kicker. Defense AWA: Player 19, Test Quarterback, Test Kicker. LoS is HOM 50.", 
                result.Environment.CurrentPlayContext!.LastPlayDescriptionTemplate);
        }

        private static GameContext CreateTestGameContext(
            GameTeam teamWithPossession, 
            PlayInvolvement playInvolvement, 
            int rosterSize = 20)
        {
            var randomFactory = new Mock<IRandomFactory>();
            randomFactory.Setup(rf => rf.Create()).Returns(new Mock<IRandom>().Object);

            var homeRoster = CreateTestRoster(rosterSize, isHomeTeam: true);
            var awayRoster = CreateTestRoster(rosterSize, isHomeTeam: false);

            var gameRecord = new GameRecord
            {
                GameID = 1,
                HomeTeam = new Team { TeamID = 1, TeamName = "Home Team", Abbreviation = "HOM", CityName = "City" },
                AwayTeam = new Team { TeamID = 2, TeamName = "Away Team", Abbreviation = "AWA", CityName = "City" }
            };

            var playContext = TestHelpers.EmptyPlayContext with
            {
                TeamWithPossession = teamWithPossession,
                PlayInvolvement = playInvolvement
            };

            var environment = new GameEnvironment
            {
                FootballRepository = Mock.Of<IFootballRepository>(),
                PhysicsParams = TestHelpers.EmptyPhysicsParams,
                DebugContextWriter = null!,
                CurrentPlayContext = playContext,
                CurrentGameRecord = gameRecord,
                RandomFactory = randomFactory.Object,
                AwayActiveRoster = awayRoster,
                HomeActiveRoster = homeRoster,
                EventBus = Mock.Of<IEventBus>()
            };

            return TestHelpers.EmptyGameContext with
            {
                TeamWithPossession = teamWithPossession,
                Environment = environment
            };
        }

        private static List<PlayerRosterPosition> CreateTestRoster(int size, bool isHomeTeam)
        {
            var roster = new List<PlayerRosterPosition>();
            var teamId = isHomeTeam ? 1 : 2;

            // Add required positions first
            roster.Add(new PlayerRosterPosition
            {
                PlayerRosterPositionID = 1,
                PlayerID = 1,
                Player = new Player
                {
                    PlayerID = 1,
                    FirstName = "Test",
                    LastName = "Quarterback"
                },
                TeamID = teamId,
                Position = BasicPlayerPosition.Quarterback,
                JerseyNumber = 1,
                CurrentPlayer = true
            });

            roster.Add(new PlayerRosterPosition
            {
                PlayerRosterPositionID = 2,
                PlayerID = 2,
                Player = new Player
                {
                    PlayerID = 2,
                    FirstName = "Test",
                    LastName = "Kicker"
                },
                TeamID = teamId,
                Position = BasicPlayerPosition.Kicker,
                JerseyNumber = 2,
                CurrentPlayer = true
            });

            // Add remaining players as offense/defense
            for (int i = 3; i <= size; i++)
            {
                roster.Add(new PlayerRosterPosition
                {
                    PlayerRosterPositionID = i,
                    PlayerID = i,
                    Player = new Player
                    {
                        PlayerID = i,
                        FirstName = "Player",
                        LastName = i.ToString()
                    },
                    TeamID = teamId,
                    Position = i % 2 == 0 ? BasicPlayerPosition.Offense : BasicPlayerPosition.Defense,
                    JerseyNumber = i,
                    CurrentPlayer = true
                });
            }

            return roster;
        }
    }
}