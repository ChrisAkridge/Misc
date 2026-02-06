using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Decisions
{
    public class KickoffDecisionTests
    {
        [Fact]
        public void KickoffDecision_Run_KickingTeamUltraConservative_PerformsNormalKickoff()
        {
            // Arrange
            var priorState = TestHelpers.EmptyPlayContext with
            {
                NextState = PlayEvaluationState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
            };
            // Act
            var newState = KickoffDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, newState.NextState);
        }

        [Fact]
        public void KickoffDecision_Run_KickingTeamConservative_DownByALotWithLittleTime_PerformsOnsideKickAttempt()
        {
            // Arrange
            var priorState = TestHelpers.EmptyPlayContext with
            {
                NextState = PlayEvaluationState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
                AwayScore = 0,
                HomeScore = 28,
                SecondsLeftInPeriod = 120, // 2 minutes left
                PeriodNumber = 4
            };
            // Act
            var newState = KickoffDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.OnsideKickAttemptOutcome, newState.NextState);
        }

        [Fact]
        public void KickoffDecision_Run_KickingTeamConservative_NotDownByALot_PerformsNormalKickoff()
        {
            // Arrange
            var priorState = TestHelpers.EmptyPlayContext with
            {
                NextState = PlayEvaluationState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
                AwayScore = 14,
                HomeScore = 21,
                SecondsLeftInPeriod = 300, // 5 minutes left
                PeriodNumber = 4
            };
            // Act
            var newState = KickoffDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, newState.NextState);
        }

        [Theory]
        [InlineData(TeamDisposition.Insane)]
        [InlineData(TeamDisposition.UltraInsane)]
        public void KickoffDecision_Run_KickingTeamInsane_PerformsOnsideKickAttempt(TeamDisposition disposition)
        {
            // Arrange
            var priorState = TestHelpers.EmptyPlayContext with
            {
                NextState = PlayEvaluationState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
                Environment = new PlayEnvironment
                {
                    DecisionParameters = new GameDecisionParameters
                    {
                        Random = null!,
                        AwayTeam = new Team
                        {
                            CityName = "Topeka",
                            TeamName = "Kansans",
                            Disposition = disposition,
                            Abbreviation = "TKA"
                        },
                        HomeTeam = null!,
                        AwayTeamActualStrengths = null!,
                        HomeTeamActualStrengths = null!,
                        AwayTeamEstimateOfAway = null!,
                        AwayTeamEstimateOfHome = null!,
                        HomeTeamEstimateOfAway = null!,
                        HomeTeamEstimateOfHome = null!,
                    },
                    PhysicsParams = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };
            // Act
            var newState = KickoffDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.OnsideKickAttemptOutcome, newState.NextState);
        }
    }
}
