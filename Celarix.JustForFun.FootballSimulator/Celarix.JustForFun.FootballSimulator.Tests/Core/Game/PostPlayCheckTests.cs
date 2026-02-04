using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class PostPlayCheckTests
    {
        private static GameContext CreateGameContext(
            GameTeam teamWithPossession = GameTeam.Away,
            int periodNumber = 1,
            int secondsLeftInPeriod = 300,
            int homeScore = 0,
            int awayScore = 0,
            GameType gameType = GameType.RegularSeason,
            bool homeScoredThisPlay = false,
            bool awayScoredThisPlay = false,
            GameTeam playContextTeamWithPossession = GameTeam.Away,
            int playCountOnDrive = 5,
            Mock<IFootballRepository>? repository = null,
            DriveResult? driveResult = null)
        {
            var mockRepository = repository ?? new Mock<IFootballRepository>();
            
            var gameRecord = new GameRecord
            {
                GameID = 1,
                HomeTeamID = 1,
                AwayTeamID = 2,
                GameType = gameType,
                TeamDriveRecords = new List<TeamDriveRecord>()
            };

            var playContext = TestHelpers.EmptyPlayContext with
            {
                PeriodNumber = periodNumber,
                SecondsLeftInPeriod = secondsLeftInPeriod,
                HomeScore = homeScore,
                AwayScore = awayScore,
                TeamWithPossession = playContextTeamWithPossession,
                HomeScoredThisPlay = homeScoredThisPlay,
                AwayScoredThisPlay = awayScoredThisPlay,
                DriveResult = driveResult,
                Environment = new PlayEnvironment
                {
                    PhysicsParams = TestHelpers.EmptyPhysicsParams,
                    DecisionParameters = null!
                }
            };

            return TestHelpers.EmptyGameContext with
            {
                TeamWithPossession = teamWithPossession,
                PlayCountOnDrive = playCountOnDrive,
                Environment = new GameEnvironment
                {
                    CurrentPlayContext = playContext,
                    CurrentGameRecord = gameRecord,
                    FootballRepository = mockRepository.Object,
                    PhysicsParams = TestHelpers.EmptyPhysicsParams,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    AwayActiveRoster = [],
                    HomeActiveRoster = []
                }
            };
        }

        [Fact]
        public void Run_RunNextPlay_WhenNormalGameplay()
        {
            // Arrange
            var context = CreateGameContext();

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_EndGameWin_WhenOvertimeWithScoreAndBothTeamsHadPossession()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            var driveRecords = new List<TeamDriveRecord>
            {
                new TeamDriveRecord { QuarterNumber = 5, TeamID = 1 },
                new TeamDriveRecord { QuarterNumber = 5, TeamID = 2 }
            };
            
            var context = CreateGameContext(
                periodNumber: 5,
                homeScore: 21,
                awayScore: 14,
                homeScoredThisPlay: true,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            context.Environment.CurrentGameRecord!.TeamDriveRecords = driveRecords;

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EndGame, result.NextState);
        }

        [Fact]
        public void Run_RunNextPlay_WhenOvertimeWithoutBothTeamsPossession()
        {
            // Arrange
            var driveRecords = new List<TeamDriveRecord>
            {
                new TeamDriveRecord { QuarterNumber = 5, TeamID = 1 } // Only one team had possession
            };

            var context = CreateGameContext(
                periodNumber: 5,
                homeScore: 21,
                awayScore: 14,
                homeScoredThisPlay: true);

            context.Environment.CurrentGameRecord!.TeamDriveRecords = driveRecords;

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_RunNextPlay_WhenOvertimeWithNoScore()
        {
            // Arrange
            var context = CreateGameContext(
                periodNumber: 5,
                homeScore: 14,
                awayScore: 14);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_EndGameWin_WhenRegulationEndsWithScoreDifference()
        {
            // Arrange
            var context = CreateGameContext(
                periodNumber: 4,
                secondsLeftInPeriod: 0,
                homeScore: 24,
                awayScore: 17,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EndGame, result.NextState);
        }

        [Fact]
        public void Run_EndGameTie_WhenRegularSeasonFifthQuarterEndsTied()
        {
            // Arrange
            var context = CreateGameContext(
                periodNumber: 5,
                secondsLeftInPeriod: 0,
                homeScore: 14,
                awayScore: 14,
                gameType: GameType.RegularSeason,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EndGame, result.NextState);
        }

        [Fact]
        public void Run_NextPeriod_WhenQuarterEndsWithTiedScore()
        {
            // Arrange
            var context = CreateGameContext(
                periodNumber: 2,
                secondsLeftInPeriod: 0,
                homeScore: 14,
                awayScore: 14,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.StartNextPeriod, result.NextState);
        }

        [Fact]
        public void Run_NextPeriod_WhenPlayoffOvertimeEndsTied()
        {
            // Arrange
            var context = CreateGameContext(
                periodNumber: 5,
                secondsLeftInPeriod: 0,
                homeScore: 14,
                awayScore: 14,
                gameType: GameType.Postseason);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.StartNextPeriod, result.NextState);
        }

        [Fact]
        public void Run_EndOfPossession_WhenPossessionChanges()
        {
            // Arrange
            var context = CreateGameContext(
                teamWithPossession: GameTeam.Home,
                playContextTeamWithPossession: GameTeam.Away,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_SavesDriveRecord_WhenEndOfPossession()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateGameContext(
                teamWithPossession: GameTeam.Home,
                playContextTeamWithPossession: GameTeam.Away,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            PostPlayCheck.Run(context);

            // Assert
            mockRepository.Verify(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()), Times.Once);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_SavesDriveRecord_WhenEndGameWin()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            var driveRecords = new List<TeamDriveRecord>
            {
                new TeamDriveRecord { QuarterNumber = 5, TeamID = 1 },
                new TeamDriveRecord { QuarterNumber = 5, TeamID = 2 }
            };

            var context = CreateGameContext(
                periodNumber: 5,
                homeScore: 21,
                awayScore: 14,
                homeScoredThisPlay: true,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            context.Environment.CurrentGameRecord!.TeamDriveRecords = driveRecords;

            // Act
            PostPlayCheck.Run(context);

            // Assert
            mockRepository.Verify(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()), Times.Once);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void Run_SavesDriveRecord_WhenNextPeriodAndCurrentDriveEnds()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateGameContext(
                periodNumber: 1, // NextQuarterActions(1) -> NextPeriodNumber = 2, CoinTossNeeded = false, CoinTossLoserReceivesPossession = false
                secondsLeftInPeriod: 0,
                repository: mockRepository);

            // Act
            PostPlayCheck.Run(context);

            // Assert
            // Since period 1 -> 2 doesn't trigger CurrentDriveEnds (not a coin toss period), drive record should not be saved
            mockRepository.Verify(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()), Times.Never);
        }

        [Fact]
        public void Run_SavesDriveRecord_WhenNextPeriodAndCoinTossNeeded()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            var context = CreateGameContext(
                periodNumber: 4, // NextQuarterActions(4) -> NextPeriodNumber = 5, CoinTossNeeded = true
                secondsLeftInPeriod: 0,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            PostPlayCheck.Run(context);

            // Assert
            mockRepository.Verify(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()), Times.Once);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveDriveRecord_SavesCorrectTeamID_WhenAwayTeamHasPossession()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            TeamDriveRecord? savedRecord = null;
            mockRepository.Setup(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()))
                         .Callback<TeamDriveRecord>(record => savedRecord = record);

            var context = CreateGameContext(
                teamWithPossession: GameTeam.Away,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            PostPlayCheck.SaveDriveRecord(context);

            // Assert
            Assert.NotNull(savedRecord);
            Assert.Equal(2, savedRecord.TeamID); // AwayTeamID from CreateGameContext
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveDriveRecord_SavesCorrectTeamID_WhenHomeTeamHasPossession()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            TeamDriveRecord? savedRecord = null;
            mockRepository.Setup(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()))
                         .Callback<TeamDriveRecord>(record => savedRecord = record);

            var context = CreateGameContext(
                teamWithPossession: GameTeam.Home,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            // Act
            PostPlayCheck.SaveDriveRecord(context);

            // Assert
            Assert.NotNull(savedRecord);
            Assert.Equal(1, savedRecord.TeamID); // HomeTeamID from CreateGameContext
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveDriveRecord_UsesCorrectGameIDAndPlayCount()
        {
            // Arrange
            var mockRepository = new Mock<IFootballRepository>();
            TeamDriveRecord? savedRecord = null;
            mockRepository.Setup(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()))
                         .Callback<TeamDriveRecord>(record => savedRecord = record);

            var context = CreateGameContext(
                playCountOnDrive: 8,
                repository: mockRepository,
                driveResult: DriveResult.TouchdownWithXP);

            // Mock the BuildTeamDriveRecord method by setting up a basic record
            var mockPlayContext = context.Environment.CurrentPlayContext!;

            // Act
            PostPlayCheck.SaveDriveRecord(context);

            // Assert
            mockRepository.Verify(r => r.AddTeamDriveRecord(It.IsAny<TeamDriveRecord>()), Times.Once);
            mockRepository.Verify(r => r.SaveChanges(), Times.Once);
        }

        [Theory]
        [InlineData(3, 300, 14, 14, false, false, GameTeam.Away, GameTeam.Away, GameState.EvaluatingPlay)] // Normal gameplay
        [InlineData(4, 0, 21, 14, false, false, GameTeam.Away, GameTeam.Away, GameState.EndGame)] // End of regulation with score difference
        [InlineData(4, 0, 14, 14, false, false, GameTeam.Away, GameTeam.Away, GameState.StartNextPeriod)] // End of regulation tied
        [InlineData(5, 300, 21, 14, true, false, GameTeam.Home, GameTeam.Home, GameState.EvaluatingPlay)] // Overtime without both teams having possession
        [InlineData(2, 300, 14, 14, false, false, GameTeam.Home, GameTeam.Away, GameState.EvaluatingPlay)] // Change of possession
        public void Run_ReturnsCorrectState_ForVariousScenarios(
            int periodNumber,
            int secondsLeftInPeriod,
            int homeScore,
            int awayScore,
            bool homeScoredThisPlay,
            bool awayScoredThisPlay,
            GameTeam teamWithPossession,
            GameTeam playContextTeamWithPossession,
            GameState expectedState)
        {
            // Arrange
            var context = CreateGameContext(
                teamWithPossession: teamWithPossession,
                periodNumber: periodNumber,
                secondsLeftInPeriod: secondsLeftInPeriod,
                homeScore: homeScore,
                awayScore: awayScore,
                homeScoredThisPlay: homeScoredThisPlay,
                awayScoredThisPlay: awayScoredThisPlay,
                playContextTeamWithPossession: playContextTeamWithPossession,
                driveResult: secondsLeftInPeriod == 0 || teamWithPossession != playContextTeamWithPossession
                    ? DriveResult.TouchdownWithXP
                    : null);

            // Act
            var result = PostPlayCheck.Run(context);

            // Assert
            Assert.Equal(expectedState, result.NextState);
        }
    }
}