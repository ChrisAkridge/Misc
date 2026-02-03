using Celarix.JustForFun.FootballSimulator.Core.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class ErrorStepTests
    {
        [Fact]
        public void Run_ShouldDoNothing()
        {
            // Arrange
            var systemContext = TestHelpers.EmptySystemContext with
            {
                NextState = SystemState.Error
            };

            // Act
            ErrorStep.Run(systemContext);

            // Assert
            Assert.Equal(SystemState.Error, systemContext.NextState);
        }
    }
}
