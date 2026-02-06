using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class EvaluatingPlayStepTests
    {
        private static Team CreateTestTeam(int teamId, string abbreviation)
        {
            var team = new Team
            {
                TeamID = teamId,
                CityName = "Test",
                TeamName = "Team",
                Abbreviation = abbreviation,
                Disposition = TeamDisposition.Conservative
            };
            TestHelpers.SetFixedStrengths(team, 75.0);
            return team;
        }

        private static GameContext CreateTestGameContext(PlayEvaluationState playState, NextPlayKind nextPlay = NextPlayKind.FreeKick)
        {
            var awayTeam = CreateTestTeam(1, "AWY");
            var homeTeam = CreateTestTeam(2, "HOM");

            var physicsParams = new Dictionary<string, PhysicsParam>
            {
                ["StrengthEstimatorOffsetMean"] = new PhysicsParam("StrengthEstimatorOffsetMean", 0, "offset", "offset"),
                ["StrengthEstimatorOffsetStddev"] = new PhysicsParam("StrengthEstimatorOffsetStddev", 0.1, "offset", "offset"),
                ["StrengthEstimatorConservativeAdjustment"] = new PhysicsParam("StrengthEstimatorConservativeAdjustment", 0, "offset", "offset"),
                ["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInStandardZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay", 1.0, "multiple", "multiple"),
                ["OnsideKickPointsPerMinuteThreshold"] = new PhysicsParam("OnsideKickPointsPerMinuteThreshold", 0.5, "points per minute", "points per minute"),
                ["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInStandardZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
            };

            var decisionParams = new GameDecisionParameters
            {
                AwayTeam = awayTeam,
                HomeTeam = homeTeam,
                AwayTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(awayTeam, GameTeam.Away),
                HomeTeamActualStrengths = TeamStrengthSet.FromTeamDirectly(homeTeam, GameTeam.Home),
                AwayTeamEstimateOfAway = TeamStrengthSet.FromTeamDirectly(awayTeam, GameTeam.Away),
                AwayTeamEstimateOfHome = TeamStrengthSet.FromTeamDirectly(homeTeam, GameTeam.Home),
                HomeTeamEstimateOfAway = TeamStrengthSet.FromTeamDirectly(awayTeam, GameTeam.Away),
                HomeTeamEstimateOfHome = TeamStrengthSet.FromTeamDirectly(homeTeam, GameTeam.Home),
                Random = new Mock<IRandom>().Object
            };

            var playEnvironment = new PlayEnvironment
            {
                DecisionParameters = decisionParams,
                PhysicsParams = physicsParams,
                EventBus = Mock.Of<IEventBus>()
            };

            var playContext = TestHelpers.EmptyPlayContext with
            {
                NextState = playState,
                NextPlay = nextPlay,
                TeamWithPossession = GameTeam.Home,
                Environment = playEnvironment
            };

            var gameEnvironment = new GameEnvironment
            {
                CurrentPlayContext = playContext,
                CurrentGameRecord = new GameRecord
                {
                    GameID = 1,
                    AwayTeam = awayTeam,
                    HomeTeam = homeTeam
                },
                FootballRepository = new Mock<IFootballRepository>().Object,
                PhysicsParams = physicsParams,
                RandomFactory = new Mock<IRandomFactory>().Object,
                AwayActiveRoster = Array.Empty<PlayerRosterPosition>(),
                HomeActiveRoster = Array.Empty<PlayerRosterPosition>(),
                DebugContextWriter = new Mock<IDebugContextWriter>().Object,
                EventBus = Mock.Of<IEventBus>()
            };

            GameContext gameContext = TestHelpers.EmptyGameContext with
            {
                Environment = gameEnvironment,
                TeamWithPossession = GameTeam.Home,
                PlayCountOnDrive = 1
            };
            Helpers.RebuildStrengthsInDecisionParameters(gameContext, decisionParams.Random);

            return gameContext;
        }

        [Fact]
        public void Run_FreeKickDecision_WhenKickDefenseStrongerThanKickReturn_ChoosesNormalKickoffOutcome()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.FreeKickDecision);
            
            // Set up stronger kick defense than kick return
            var homeTeam = context.Environment.CurrentGameRecord!.HomeTeam;
            homeTeam!.KickDefenseStrength = 80.0;
            homeTeam.KickReturnStrength = 70.0;

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, context.Environment.CurrentPlayContext!.NextState);
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_FreeKickDecision_WhenKickReturnStrongerThanKickDefense_ChoosesPuntOutcome()
        {
            // Arrange
            var random = new Mock<IRandom>();
            random.Setup(r => r.NextDouble()).Returns(0.5);

            var context = CreateTestGameContext(PlayEvaluationState.FreeKickDecision);
            
            // Set up stronger kick return than kick defense
            var homeTeam = context.Environment.CurrentGameRecord!.HomeTeam;
            var awayTeam = context.Environment.CurrentGameRecord!.AwayTeam;
            homeTeam!.KickDefenseStrength = 70.0;
            awayTeam!.KickReturnStrength = 80.0;
            Helpers.RebuildStrengthsInDecisionParameters(context, random.Object);

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(PlayEvaluationState.PuntOutcome, context.Environment.CurrentPlayContext!.NextState);
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_PlayEvaluationComplete_ResetsPlayContextForFreeKick()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.FreeKick);

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            // We still advance the play evaluation state even when the next play starts
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, context.Environment.CurrentPlayContext!.NextState);
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
            Assert.Equal(1, result.PlayCountOnDrive); // Should reset to 1 for free kick
        }

        [Fact]
        public void Run_PlayEvaluationComplete_ResetsPlayContextForKickoff()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.Kickoff);

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, context.Environment.CurrentPlayContext!.NextState);
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
            Assert.Equal(1, result.PlayCountOnDrive); // Should reset to 1 for kickoff
        }

        [Fact]
        public void Run_PlayEvaluationComplete_SetsMainGameDecisionForDowns()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.FirstDown);

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(PlayEvaluationState.StandardRushingPlayOutcome, context.Environment.CurrentPlayContext!.NextState);
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_PlayEvaluationComplete_IncrementsPlayCountWhenNotResetting()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.SecondDown) with
            {
                PlayCountOnDrive = 3
            };

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(4, result.PlayCountOnDrive); // Should increment from 3 to 4
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
        }

        [Fact]
        public void Run_PlayEvaluationComplete_ResetsPlayContextProperties()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.FreeKick);
            var originalPlayContext = context.Environment.CurrentPlayContext!;

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);
            var newPlayContext = context.Environment.CurrentPlayContext!;

            // Assert
            Assert.False(newPlayContext.AwayScoredThisPlay);
            Assert.False(newPlayContext.HomeScoredThisPlay);
            Assert.Null(newPlayContext.DriveResult);
            Assert.Equal(PossessionOnPlay.None, newPlayContext.PossessionOnPlay);
            Assert.Null(newPlayContext.TeamCallingTimeout);
            Assert.NotNull(newPlayContext.PlayInvolvement);
        }

        [Fact]
        public void Run_PlayEvaluationComplete_WhenPossessionChanges_ResetsPlayCount()
        {
            // Arrange
            var playContext = TestHelpers.EmptyPlayContext with
            {
                NextState = PlayEvaluationState.PlayEvaluationComplete,
                NextPlay = NextPlayKind.FirstDown,
                TeamWithPossession = GameTeam.Away, // Different from context
                Environment = new PlayEnvironment
                {
                    DecisionParameters = new GameDecisionParameters
                    {
                        AwayTeam = CreateTestTeam(1, "AWY"),
                        HomeTeam = CreateTestTeam(2, "HOM"),
                        Random = Mock.Of<IRandom>(),
                        AwayTeamActualStrengths = null!,
                        HomeTeamActualStrengths = null!,
                        HomeTeamEstimateOfAway = null!,
                        HomeTeamEstimateOfHome = null!,
                        AwayTeamEstimateOfAway = null!,
                        AwayTeamEstimateOfHome = null!
                    },
                    PhysicsParams = new Dictionary<string, PhysicsParam>
                    {
                        ["StrengthEstimatorOffsetMean"] = new PhysicsParam("StrengthEstimatorOffsetMean", 0, "offset", "offset"),
                        ["StrengthEstimatorOffsetStddev"] = new PhysicsParam("StrengthEstimatorOffsetStddev", 0.1, "offset", "offset"),
                        ["StrengthEstimatorConservativeAdjustment"] = new PhysicsParam("StrengthEstimatorConservativeAdjustment", 0, "offset", "offset"),
                        ["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInStandardZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                        ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                        ["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay"] = new PhysicsParam("LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay", 1.0, "multiple", "multiple"),
                        ["OnsideKickPointsPerMinuteThreshold"] = new PhysicsParam("OnsideKickPointsPerMinuteThreshold", 0.5, "points per minute", "points per minute"),
                        ["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"] = new PhysicsParam("LeadingClockDispositionInStandardZoneOpponentStrengthMultiple", 1.0, "multiple", "multiple"),
                    },
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete, NextPlayKind.FirstDown) with
            {
                PlayCountOnDrive = 5,
                TeamWithPossession = GameTeam.Home
            };
            context.Environment.CurrentPlayContext = playContext;
            Helpers.RebuildStrengthsInDecisionParameters(context, Mock.Of<IRandom>());

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(1, result.PlayCountOnDrive); // Should reset to 1 when possession changes
        }

        [Fact]
        public void Run_InProgressPlayState_ReturnsCorrectSignalAndState()
        {
            // Arrange - Using a non-complete state
            var context = CreateTestGameContext(PlayEvaluationState.FreeKickDecision);

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(EvaluatingPlaySignal.InProgress, signal);
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }

        [Fact]
        public void Run_PlayEvaluationCompleteState_ReturnsCorrectSignalAndNextState()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.PlayEvaluationComplete);
            // Simulate the play context returning to PlayEvaluationComplete after processing
            var mockPlayContext = context.Environment.CurrentPlayContext! with 
            { 
                NextState = PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision
            };
            context.Environment.CurrentPlayContext = mockPlayContext;
            // Set both teams to UltraConservative to force early exit
            context.Environment.CurrentPlayContext.Environment!.DecisionParameters.AwayTeam.Disposition = TeamDisposition.UltraConservative;
            context.Environment.CurrentPlayContext.Environment.DecisionParameters.HomeTeam.Disposition = TeamDisposition.UltraConservative;

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(EvaluatingPlaySignal.PlayEvaluationComplete, signal);
            Assert.Equal(GameState.AdjustStrengths, result.NextState);
        }

        [Fact]
        public void Run_PreservesContextEnvironment()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.FreeKickDecision);
            var originalEnvironment = context.Environment;

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Same(originalEnvironment, result.Environment);
        }

        [Fact]
        public void Run_IncrementsVersion()
        {
            // Arrange
            var context = CreateTestGameContext(PlayEvaluationState.FreeKickDecision) with { Version = 10 };

            // Act
            var result = EvaluatingPlayStep.Run(context, out var signal);

            // Assert
            Assert.Equal(11, result.Version);
        }
    }
}