using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Moq;
using System;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Decisions
{
    public class FreeKickDecisionTests
    {
        [Fact]
        public void Run_KickingTeamBetterAtKickDefense_ChoosesNormalKickoffOutcome()
        {
            // Arrange
            var priorState = TestHelpers.EmptyState;
            var parameters = new GameDecisionParameters
            {
                AwayTeamEstimateOfAway = new TeamStrengthSet
                {
                    KickDefenseStrength = 60
                },
                AwayTeamEstimateOfHome = new TeamStrengthSet
                {
                    KickReturnStrength = 50
                }
            };
            // Act
            var resultState = FreeKickDecision.Run(
                priorState,
                parameters,
                physicsParams: TestHelpers.EmptyPhysicsParams);
            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, resultState.NextState);
        }

        [Fact]
        public void Run_OpponentBetterAtKickReturns_ChoosesPuntOutcome()
        {
            // Arrange
            var priorState = TestHelpers.EmptyState;
            var parameters = new GameDecisionParameters
            {
                AwayTeamEstimateOfAway = new TeamStrengthSet
                {
                    KickDefenseStrength = 40
                },
                AwayTeamEstimateOfHome = new TeamStrengthSet
                {
                    KickReturnStrength = 50
                }
            };
            // Act
            var resultState = FreeKickDecision.Run(
                priorState,
                parameters,
                physicsParams: TestHelpers.EmptyPhysicsParams);
            // Assert
            Assert.Equal(PlayEvaluationState.PuntOutcome, resultState.NextState);
        }
    }
}