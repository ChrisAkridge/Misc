using Celarix.JustForFun.FootballSimulator.Core.Game;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Tests.Core;
using Serilog;
using Serilog.Events;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.Game
{
    public class StartStepTests
    {
        [Fact]
        public void Run_ReturnsContextWithEvaluatingPlayState()
        {
            // Arrange
            var inputContext = TestHelpers.EmptyGameContext with
            {
                NextState = GameState.Start,
                Version = 5L
            };

            // Act
            var result = StartStep.Run(inputContext);

            // Assert
            Assert.Equal(GameState.EvaluatingPlay, result.NextState);
        }
    }
}