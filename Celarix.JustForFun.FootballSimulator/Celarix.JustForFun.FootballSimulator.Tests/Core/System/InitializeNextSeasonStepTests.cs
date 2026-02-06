using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class InitializeNextSeasonStepTests
    {
        [Fact]
        public void Run_InitializesFirstSeasonCorrectly()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            var teams = SeedData.TeamSeedData();
            var randomFactory = new Mock<IRandomFactory>();

            for (var i = 0; i < teams.Count; i++)
            {
                teams[i].TeamID = i + 1;
            }

            repository.Setup(r => r.GetTeams()).Returns(teams);

            var receivedGames = new List<GameRecord>();
            SeasonRecord? receivedSeason = null;
            repository.Setup(r => r.AddGameRecords(It.IsAny<IEnumerable<GameRecord>>()))
                .Callback<IEnumerable<GameRecord>>(receivedGames.AddRange);
            repository.Setup(r => r.AddSeasonRecord(It.IsAny<SeasonRecord>()))
                .Callback<SeasonRecord>(sr => receivedSeason = sr);

            randomFactory.Setup(rf => rf.Create()).Returns(new RandomWrapper());
            randomFactory.Setup(rf => rf.Create(It.IsAny<int>())).Returns<int>(seed => new RandomWrapper(seed));

            var context = TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = randomFactory.Object,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeNextSeasonStep.Run(context);

            // Assert
            repository.Verify(r => r.SaveChanges(), Times.Once);

            Assert.NotNull(receivedSeason);
            Assert.Equal(2014, receivedSeason!.Year);

            const int preseasonGames = 80;
            const int regularSeasonGames = 320;
            Assert.Equal(preseasonGames + regularSeasonGames, receivedGames.Count);
            Assert.All(teams, team =>
            {
                var teamGames = receivedGames.FindAll(gr =>
                    gr.HomeTeamID == team.TeamID || gr.AwayTeamID == team.TeamID);
                Assert.Equal(20, teamGames.Count);  // 16 regular season + 4 preseason
            });
        }

        [Fact]
        public void Run_InitializesNewSeasonCorrectly()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            var teams = SeedData.TeamSeedData();
            var randomFactory = new Mock<IRandomFactory>();

            for (var i = 0; i < teams.Count; i++)
            {
                teams[i].TeamID = i + 1;
            }

            repository.Setup(r => r.GetTeams()).Returns(teams);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2018
            });
            repository.Setup(r => r.GetGameRecordsForSeasonByGameType(1, GameType.RegularSeason))
                .Returns([]);
            repository.Setup(r => r.GetPlayoffGamesForSeason(1, PlayoffRound.SuperBowl))
                .Returns(
                [
                    new GameRecord
                    {
                        QuarterBoxScores =
                        [
                            new QuarterBoxScore
                            {
                                Team = GameTeam.Home,
                                Score = 24
                            }
                        ],
                        HomeTeam = teams[1]
                    }
                ]);

            var receivedGames = new List<GameRecord>();
            SeasonRecord? receivedSeason = null;
            repository.Setup(r => r.AddGameRecords(It.IsAny<IEnumerable<GameRecord>>()))
                .Callback<IEnumerable<GameRecord>>(receivedGames.AddRange);
            repository.Setup(r => r.AddSeasonRecord(It.IsAny<SeasonRecord>()))
                .Callback<SeasonRecord>(sr => receivedSeason = sr);

            randomFactory.Setup(rf => rf.Create()).Returns(new RandomWrapper());
            randomFactory.Setup(rf => rf.Create(It.IsAny<int>())).Returns<int>(seed => new RandomWrapper(seed));

            var context = TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = randomFactory.Object,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeNextSeasonStep.Run(context);

            // Assert
            repository.Verify(r => r.SaveChanges(), Times.Once);

            Assert.NotNull(receivedSeason);
            Assert.Equal(2019, receivedSeason!.Year);

            const int preseasonGames = 80;
            const int regularSeasonGames = 320;
            Assert.Equal(preseasonGames + regularSeasonGames, receivedGames.Count);
            Assert.All(teams, team =>
            {
                var teamGames = receivedGames.FindAll(gr =>
                    gr.HomeTeamID == team.TeamID || gr.AwayTeamID == team.TeamID);
                Assert.Equal(20, teamGames.Count);  // 16 regular season + 4 preseason
            });

            var kickoffGame = receivedGames.Where(g => g.GameType == GameType.RegularSeason)
                .OrderBy(g => g.KickoffTime)
                .First();
            var homeTeam = teams.Single(t => t.TeamID == kickoffGame.HomeTeamID);
            Assert.Equal(teams[1].TeamName, homeTeam.TeamName);
        }
    }
}
