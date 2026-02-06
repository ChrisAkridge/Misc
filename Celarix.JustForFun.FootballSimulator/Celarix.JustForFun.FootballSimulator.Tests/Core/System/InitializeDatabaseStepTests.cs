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
    public class InitializeDatabaseStepTests
    {
        [Fact]
        public void Run_InitializesDatabase()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();

            SimulatorSettings? receivedSettings = null;
            repository.Setup(r => r.AddSimulatorSettings(It.IsAny<SimulatorSettings>()))
                .Callback<SimulatorSettings>(settings => receivedSettings = settings);

            var firstNamesAllFirst = true;
            var lastNamesAllLast = true;
            repository.Setup(r => r.AddPlayer(It.IsAny<Player>()))
                .Callback<Player>(player =>
                {
                    if (!player.FirstName.StartsWith("First"))
                    {
                        firstNamesAllFirst = false;
                    }
                    if (!player.LastName.StartsWith("Last"))
                    {
                        lastNamesAllLast = false;
                    }
                });

            var randomFactory = new Mock<IRandomFactory>();
            var random = new Mock<IRandom>();
            random.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((min, max) => min); // Always pick the first name in the list
            random.Setup(r => r.Choice(It.IsAny<IReadOnlyList<string>>()))
                .Returns<IReadOnlyList<string>>(l => l[0]);
            randomFactory.Setup(rf => rf.Create()).Returns(random.Object);

            var context = TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    PlayerFactory = new PlayerFactory(["First"], ["Last"]),
                    RandomFactory = randomFactory.Object,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeDatabaseStep.Run(context);

            // Assert
            repository.Verify(r => r.AddTeam(It.IsAny<Team>()), Times.Exactly(40));
            repository.Verify(r => r.AddPlayer(It.IsAny<Player>()), Times.Exactly(40 * 23));
            repository.Verify(r => r.AddPhysicsParam(It.IsAny<PhysicsParam>()), Times.Exactly(SeedData.ParamSeedData().Count));
            repository.Verify(r => r.SaveChanges(), Times.Exactly(4));

            Assert.NotNull(receivedSettings);
            Assert.True(receivedSettings.SeedDataInitialized);
            Assert.Equal(SystemState.InitializeNextSeason, step.NextState);
            Assert.True(firstNamesAllFirst);
            Assert.True(lastNamesAllLast);
        }

        [Fact]
        public void Run_UsesExistingSeedData()
        {
            // Arrange
            var repository = new Mock<IFootballRepository>();
            var settings = new SimulatorSettings
            {
                SeedDataInitialized = false,
                StateMachineContextSavePath = ""
            };
            repository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            var firstNamesAllFirst = true;
            var lastNamesAllLast = true;
            repository.Setup(r => r.AddPlayer(It.IsAny<Player>()))
                .Callback<Player>(player =>
                {
                    if (!player.FirstName.StartsWith("First"))
                    {
                        firstNamesAllFirst = false;
                    }
                    if (!player.LastName.StartsWith("Last"))
                    {
                        lastNamesAllLast = false;
                    }
                });

            var randomFactory = new Mock<IRandomFactory>();
            var random = new Mock<IRandom>();
            random.Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
                .Returns<int, int>((min, max) => min); // Always pick the first name in the list
            random.Setup(r => r.Choice(It.IsAny<IReadOnlyList<string>>()))
                .Returns<IReadOnlyList<string>>(l => l[0]);
            randomFactory.Setup(rf => rf.Create()).Returns(random.Object);

            var context = TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    PlayerFactory = new PlayerFactory(["First"], ["Last"]),
                    RandomFactory = randomFactory.Object,
                    SummaryWriter = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            };

            // Act
            var step = InitializeDatabaseStep.Run(context);

            // Assert
            repository.Verify(r => r.AddTeam(It.IsAny<Team>()), Times.Exactly(40));
            repository.Verify(r => r.AddPlayer(It.IsAny<Player>()), Times.Exactly(40 * 23));
            repository.Verify(r => r.AddPhysicsParam(It.IsAny<PhysicsParam>()), Times.Exactly(SeedData.ParamSeedData().Count));
            repository.Verify(r => r.SaveChanges(), Times.Exactly(3));

            Assert.True(settings.SeedDataInitialized);
            Assert.Equal(SystemState.InitializeNextSeason, step.NextState);
            Assert.True(firstNamesAllFirst);
            Assert.True(lastNamesAllLast);
        }
    }
}
