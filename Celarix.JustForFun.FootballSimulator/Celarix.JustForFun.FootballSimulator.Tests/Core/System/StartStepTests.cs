using Celarix.JustForFun.FootballSimulator.Core.System;
using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Moq;
using Serilog;
using Xunit;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core.System
{
    public class StartStepTests
    {
        private readonly Mock<IFootballRepository> _mockRepository;
        private readonly Mock<ILogger> _mockLogger;
        private readonly SystemEnvironment _environment;
        private readonly SystemContext _context;

        public StartStepTests()
        {
            _mockRepository = new Mock<IFootballRepository>();
            _mockLogger = new Mock<ILogger>();
            
            _environment = new SystemEnvironment
            {
                FootballRepository = _mockRepository.Object,
                RandomFactory = null!,
                PlayerFactory = null!,
                DebugContextWriter = null!,
                SummaryWriter = null!,
                EventBus = Mock.Of<IEventBus>()
            };
            
            _context = new SystemContext(
                Version: 1L,
                NextState: SystemState.Start,
                Environment: _environment
            );

            // Setup Serilog mock for static logging calls
            Log.Logger = _mockLogger.Object;
        }

        [Fact]
        public void Run_WhenSettingsIsNull_ShouldMoveToInitializeDatabaseState()
        {
            // Arrange
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns((SimulatorSettings?)null);

            // Act
            var result = StartStep.Run(_context);

            // Assert
            Assert.Equal(SystemState.InitializeDatabase, result.NextState);
            _mockRepository.Verify(r => r.EnsureCreated(), Times.Once);
            _mockRepository.Verify(r => r.GetSimulatorSettings(), Times.Once);
        }

        [Fact]
        public void Run_WhenSeedDataInitializedIsFalse_ShouldMoveToInitializeDatabaseState()
        {
            // Arrange
            var settings = new SimulatorSettings
            {
                SeedDataInitialized = false,
                SaveStateMachineContextsForDebugging = false,
                StateMachineContextSavePath = ""
            };

            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            var result = StartStep.Run(_context);

            // Assert
            Assert.Equal(SystemState.InitializeDatabase, result.NextState);
            _mockRepository.Verify(r => r.EnsureCreated(), Times.Once);
            _mockRepository.Verify(r => r.GetSimulatorSettings(), Times.Once);
        }

        [Fact]
        public void Run_WhenSeedDataInitializedIsTrue_ShouldMoveToPrepareForGameState()
        {
            // Arrange
            var settings = new SimulatorSettings
            {
                SeedDataInitialized = true,
                SaveStateMachineContextsForDebugging = false,
                StateMachineContextSavePath = ""
            };

            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            var result = StartStep.Run(_context);

            // Assert
            Assert.Equal(SystemState.PrepareForGame, result.NextState);
            _mockRepository.Verify(r => r.EnsureCreated(), Times.Once);
            _mockRepository.Verify(r => r.GetSimulatorSettings(), Times.Once);
        }

        [Fact]
        public void Run_ShouldAlwaysCallEnsureCreatedOnRepository()
        {
            // Arrange
            var settings = new SimulatorSettings { SeedDataInitialized = true };
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            StartStep.Run(_context);

            // Assert
            _mockRepository.Verify(r => r.EnsureCreated(), Times.Once);
        }

        [Fact]
        public void Run_ShouldAlwaysCallGetSimulatorSettingsOnRepository()
        {
            // Arrange
            var settings = new SimulatorSettings { SeedDataInitialized = true };
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            StartStep.Run(_context);

            // Assert
            _mockRepository.Verify(r => r.GetSimulatorSettings(), Times.Once);
        }

        [Fact]
        public void Run_ShouldIncrementVersionFromInputContext()
        {
            // Arrange
            const long expectedVersion = 43L;
            var contextWithVersion = new SystemContext(
                Version: expectedVersion - 1,
                NextState: SystemState.Start,
                Environment: _environment
            );

            var settings = new SimulatorSettings { SeedDataInitialized = true };
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            var result = StartStep.Run(contextWithVersion);

            // Assert
            Assert.Equal(expectedVersion, result.Version);
        }

        [Fact]
        public void Run_ShouldPreserveEnvironmentFromInputContext()
        {
            // Arrange
            var settings = new SimulatorSettings { SeedDataInitialized = true };
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            var result = StartStep.Run(_context);

            // Assert
            Assert.Same(_environment, result.Environment);
        }

        [Theory]
        [InlineData(true, SystemState.PrepareForGame)]
        [InlineData(false, SystemState.InitializeDatabase)]
        public void Run_ShouldSetCorrectNextStateBasedOnSeedDataInitialized(bool seedDataInitialized, SystemState expectedNextState)
        {
            // Arrange
            var settings = new SimulatorSettings { SeedDataInitialized = seedDataInitialized };
            _mockRepository.Setup(r => r.EnsureCreated());
            _mockRepository.Setup(r => r.GetSimulatorSettings()).Returns(settings);

            // Act
            var result = StartStep.Run(_context);

            // Assert
            Assert.Equal(expectedNextState, result.NextState);
        }
    }
}