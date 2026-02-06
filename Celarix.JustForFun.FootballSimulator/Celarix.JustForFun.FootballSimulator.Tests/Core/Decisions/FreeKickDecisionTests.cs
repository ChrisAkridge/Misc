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
            var priorState = TestHelpers.EmptyPlayContext;
            // Act
            var resultState = FreeKickDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.NormalKickoffOutcome, resultState.NextState);
        }

        [Fact]
        public void Run_OpponentBetterAtKickReturns_ChoosesPuntOutcome()
        {
            // Arrange
            var priorState = TestHelpers.EmptyPlayContext;
            // Act
            var resultState = FreeKickDecision.Run(priorState);
            // Assert
            Assert.Equal(PlayEvaluationState.PuntOutcome, resultState.NextState);
        }
    }
}