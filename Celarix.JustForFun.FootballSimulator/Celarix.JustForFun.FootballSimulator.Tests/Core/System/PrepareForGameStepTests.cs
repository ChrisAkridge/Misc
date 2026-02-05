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
    public class PrepareForGameStepTests
    {
        [Fact]
        public void Run_PreparesForGame()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());
            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(new List<GameRecord>
            {
                new GameRecord
                {
                    GameID = 1,
                    GameComplete = false,
                    TeamDriveRecords = []
                },
                new GameRecord
                {
                    GameID = 2,
                    GameComplete = true,
                    TeamDriveRecords = []
                }
            });

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.LoadGame, step.NextState);
        }

        [Fact]
        public void Run_GoesToInitializeNextSeasonWhenNoIncompleteSeasonsExist()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(0);

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.InitializeNextSeason, step.NextState);
        }

        [Fact]
        public void Run_ThrowsWhenMultipleIncompleteSeasonsExist()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(50);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            }));
        }

        [Fact]
        public void Run_ThrowsWhenNoSeasonRecordExistsDespiteCountingIncompleteSeasons()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            }));
        }

        [Fact]
        public void Run_GoesToWriteSummaryWhenNoSummaryExistsForCompleteSeason()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.WriteSummaryForSeason, step.NextState);
        }

        [Fact]
        public void Run_ThrowsWhenMultiplePartialGamesExist()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(new List<GameRecord>
            {
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = false,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                },
                new GameRecord
                {
                    GameID = 2,
                    SeasonRecordID = 1,
                    GameComplete = false,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                }
            });

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            }));
        }

        [Fact]
        public void Run_GoesToResumePartialGameWhenPartialGameExists()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());
            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(new List<GameRecord>
            {
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = false,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                }
            });

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.ResumePartialGame, step.NextState);
        }

        [Fact]
        public void Run_GoesToInitializeWildCardRoundWhenRegularSeasonComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            List<GameRecord> seasonGames = [
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = true,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                }
            ];

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(seasonGames);

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.InitializeWildCardRound, step.NextState);
        }

        [Fact]
        public void Run_GoesToInitializeDivisionalRoundWhenWildCardComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            List<GameRecord> seasonGames = [
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = true,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                },
                .. MakePlayoffGames(8),
            ];

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(seasonGames);

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.InitializeDivisionalRound, step.NextState);
        }

        [Fact]
        public void Run_GoesToInitializeConferenceChampionshipWhenDivisionalRoundComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            List<GameRecord> seasonGames = [
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = true,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                },
                .. MakePlayoffGames(12),
            ];

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(seasonGames);

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.InitializeConferenceChampionshipRound, step.NextState);
        }

        [Fact]
        public void Run_GoesToInitializeSuperBowlWhenConferenceChampionshipComplete()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            List<GameRecord> seasonGames = [
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = true,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                },
                .. MakePlayoffGames(14),
            ];

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(seasonGames);

            // Act
            var step = PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            Assert.Equal(SystemState.InitializeSuperBowl, step.NextState);
        }

        [Fact]
        public void Run_ThrowsWhenUnexpectedNumberOfPlayoffGamesExist()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            repository.Setup(r => r.CountIncompleteSeasons()).Returns(1);
            repository.Setup(r => r.GetMostRecentSeason()).Returns(new SeasonRecord
            {
                SeasonRecordID = 1,
                Year = 2023
            });
            repository.Setup(r => r.GetSummaryForSeason(It.IsAny<int>())).Returns(new Summary());

            List<GameRecord> seasonGames = [
                new GameRecord
                {
                    GameID = 1,
                    SeasonRecordID = 1,
                    GameComplete = true,
                    TeamDriveRecords =
                    [
                        new TeamDriveRecord()
                    ]
                },
                .. MakePlayoffGames(7),
            ];

            repository.Setup(r => r.GetGameRecordsForSeason(It.IsAny<int>())).Returns(seasonGames);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => PrepareForGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    RandomFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            }));
        }

        private static IEnumerable<GameRecord> MakePlayoffGames(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new GameRecord
                {
                    GameID = i + 1,
                    GameType = GameType.Postseason,
                    GameComplete = true,
                    TeamDriveRecords = []
                };
            }
        }
    }
}
