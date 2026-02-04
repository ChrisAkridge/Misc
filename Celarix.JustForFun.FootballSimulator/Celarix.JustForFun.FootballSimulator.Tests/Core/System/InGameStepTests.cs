using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class InGameStepTests
    {
        [Fact]
        public void Run_GoesToInGame_WhenEvaluatingPlay()
        {
            // Arrange
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InGame,
                Environment = new SystemEnvironment
                {
                    FootballRepository = null!,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                    CurrentGameContext = TestHelpers.EmptyGameContext with
                    {
                        NextState = GameState.EvaluatingPlay,
                        Environment = new GameEnvironment
                        {
                            DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                            PhysicsParams = TestHelpers.EmptyPhysicsParams,
                            FootballRepository = null!,
                            RandomFactory = null!,
                            CurrentGameRecord = null!,
                            AwayActiveRoster = [],
                            HomeActiveRoster = [],
                            CurrentPlayContext = TestHelpers.EmptyPlayContext with
                            {
                                NextState = PlayEvaluationState.KickoffDecision,
                                TeamWithPossession = GameTeam.Away,
                                Environment = new PlayEnvironment
                                {
                                    DecisionParameters = new GameDecisionParameters
                                    {
                                        AwayTeam = new Team
                                        {
                                            Disposition = TeamDisposition.UltraConservative
                                        }
                                    },
                                    PhysicsParams = TestHelpers.EmptyPhysicsParams
                                }
                            }
                        }
                    }
                }
            };

            // Act
            var newContext = InGameStep.Run(context, out var inGameSignal);

            // Assert
            Assert.Equal(SystemState.InGame, newContext.NextState);
            Assert.Equal(InGameSignal.PlayEvaluationStep, inGameSignal);
        }

        [Fact]
        public void Run_GoesToPostGame_WhenEndGame()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InGame,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                    CurrentGameContext = TestHelpers.EmptyGameContext with
                    {
                        NextState = GameState.EndGame,
                        Environment = new GameEnvironment
                        {
                            CurrentPlayContext = TestHelpers.EmptyPlayContext,
                            DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                            PhysicsParams = TestHelpers.EmptyPhysicsParams,
                            FootballRepository = repository.Object,
                            RandomFactory = null!,
                            CurrentGameRecord = new GameRecord
                            {
                                GameID = 1,
                                AwayTeam = new Team
                                {
                                    TeamID = 1,
                                },
                                HomeTeam = new Team
                                {
                                    TeamID = 2,
                                }
                            },
                            AwayActiveRoster = [],
                            HomeActiveRoster = []
                        }
                    }
                }
            };
            // Act
            var newContext = InGameStep.Run(context, out var inGameSignal);
            // Assert
            Assert.Equal(SystemState.PostGame, newContext.NextState);
            Assert.Equal(InGameSignal.GameCompleted, inGameSignal);
        }

        [Fact]
        public void Run_GoesToInGame_WhenOtherGameState()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            var context = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.InGame,
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = null!,
                    PlayerFactory = null!,
                    SummaryWriter = null!,
                    DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                    CurrentGameContext = TestHelpers.EmptyGameContext with
                    {
                        NextState = GameState.StartNextPeriod,
                        Environment = new GameEnvironment
                        {
                            DebugContextWriter = new DebugContextWriter(enabled: false, ""),
                            PhysicsParams = TestHelpers.EmptyPhysicsParams,
                            FootballRepository = repository.Object,
                            RandomFactory = null!,
                            CurrentGameRecord = new GameRecord
                            {
                                AwayTeam = null!,
                                HomeTeam = null!
                            },
                            AwayActiveRoster = [],
                            HomeActiveRoster = [],
                            CurrentPlayContext = TestHelpers.EmptyPlayContext
                        }
                    }
                }
            };
            // Act
            var newContext = InGameStep.Run(context, out var inGameSignal);
            // Assert
            Assert.Equal(SystemState.InGame, newContext.NextState);
            Assert.Equal(InGameSignal.GameStateAdvanced, inGameSignal);
        }
    }
}
