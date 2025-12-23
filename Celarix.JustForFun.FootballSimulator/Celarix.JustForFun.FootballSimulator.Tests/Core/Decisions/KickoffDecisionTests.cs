using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
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
            var priorState = TestHelpers.EmptyState with
            {
                NextState = GameplayNextState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
            };
            var parameters = new Models.GameDecisionParameters
            {
                AwayTeam = new Team
                {
                    Disposition = TeamDisposition.UltraConservative
                },
            };
            var physicsParams = TestHelpers.EmptyPhysicsParams;
            // Act
            var newState = KickoffDecision.Run(priorState, parameters, physicsParams);
            // Assert
            Assert.Equal(GameplayNextState.NormalKickoffOutcome, newState.NextState);
        }

        [Fact]
        public void KickoffDecision_Run_KickingTeamConservative_DownByALotWithLittleTime_PerformsOnsideKickAttempt()
        {
            // Arrange
            var priorState = TestHelpers.EmptyState with
            {
                NextState = GameplayNextState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
                AwayScore = 0,
                HomeScore = 28,
                SecondsLeftInPeriod = 120, // 2 minutes left
                PeriodNumber = 4
            };
            var parameters = new Models.GameDecisionParameters
            {
                AwayTeam = new Team
                {
                    Disposition = TeamDisposition.Conservative
                },
            };
            var physicsParams = new Dictionary<string, PhysicsParam>
            {
                { "OnsideKickPointsPerMinuteThreshold", new PhysicsParam("OnsideKickPointsPerMinuteThreshold", 7.0, "point", "points") }
            };
            // Act
            var newState = KickoffDecision.Run(priorState, parameters, physicsParams);
            // Assert
            Assert.Equal(GameplayNextState.OnsideKickAttemptOutcome, newState.NextState);
        }

        [Fact]
        public void KickoffDecision_Run_KickingTeamConservative_NotDownByALot_PerformsNormalKickoff()
        {
            // Arrange
            var priorState = TestHelpers.EmptyState with
            {
                NextState = GameplayNextState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
                AwayScore = 14,
                HomeScore = 21,
                SecondsLeftInPeriod = 300, // 5 minutes left
                PeriodNumber = 4
            };
            var parameters = new Models.GameDecisionParameters
            {
                AwayTeam = new Team
                {
                    Disposition = TeamDisposition.Conservative
                },
            };
            var physicsParams = new Dictionary<string, PhysicsParam>
            {
                { "OnsideKickPointsPerMinuteThreshold", new PhysicsParam("OnsideKickPointsPerMinuteThreshold", 7.0, "point", "points") }
            };
            // Act
            var newState = KickoffDecision.Run(priorState, parameters, physicsParams);
            // Assert
            Assert.Equal(GameplayNextState.NormalKickoffOutcome, newState.NextState);
        }

        [Theory]
        [InlineData(TeamDisposition.Insane)]
        [InlineData(TeamDisposition.UltraInsane)]
        public void KickoffDecision_Run_KickingTeamInsane_PerformsOnsideKickAttempt(TeamDisposition disposition)
        {
            // Arrange
            var priorState = TestHelpers.EmptyState with
            {
                NextState = GameplayNextState.KickoffDecision,
                NextPlay = NextPlayKind.Kickoff,
                TeamWithPossession = GameTeam.Away,
            };
            var parameters = new Models.GameDecisionParameters
            {
                AwayTeam = new Team
                {
                    Disposition = disposition
                },
            };
            var physicsParams = TestHelpers.EmptyPhysicsParams;
            // Act
            var newState = KickoffDecision.Run(priorState, parameters, physicsParams);
            // Assert
            Assert.Equal(GameplayNextState.OnsideKickAttemptOutcome, newState.NextState);
        }
    }
}
