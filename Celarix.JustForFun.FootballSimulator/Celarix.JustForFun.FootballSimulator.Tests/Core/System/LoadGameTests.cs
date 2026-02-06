using Celarix.JustForFun.FootballSimulator.Core;
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
    public class LoadGameTests
    {
        [Fact]
        public void Run_LoadsGame()
        {
            // Arrange
            var awayTeam = new Team { HomeStadiumID = 1, CityName = "Runford", TeamName = "Runners", Abbreviation = "RUN" };
            var homeTeam = new Team { HomeStadiumID = 2, CityName = "Runford", TeamName = "Runners", Abbreviation = "RUN" };
            TestHelpers.SetRandomStrengths(awayTeam);
            TestHelpers.SetRandomStrengths(homeTeam);

            // Set a couple of strengths to known values for injury recovery testing
            awayTeam.PassingOffenseStrength = 50.0;
            homeTeam.RunningDefenseStrength = 50.0;

            var awayStadium = new Stadium
            {
                AverageTemperatures = "-90,-80,-70,-60,-50,-40,-30",
                Name = "Away Stadium",
                City = "Away City"
            };
            var homeStadium = new Stadium
            {
                AverageTemperatures = "90,80,70,60,50,40,30",
                AverageWindSpeed = 20.0,
                Name = "Home Stadium",
                City = "Home City"
            };

            var repository = new Mock<IFootballRepository>();
            List<PhysicsParam> physicsParams = SeedData.ParamSeedData();

            // Overwrite some params for testing estimated strengths
            foreach (var physicsParam in physicsParams)
            {
                if (physicsParam.Name is "StrengthEstimatorOffsetMean")
                {
                    physicsParam.Value = 0.0;
                }
                else if (physicsParam.Name is "StrengthEstimatorUltraConservativeAdjustment"
                    or "StrengthEstimatorConservativeAdjustment"
                    or "StrengthEstimatorInsaneAdjustment"
                    or "StrengthEstimatorUltraInsaneAdjustment")
                {
                    physicsParam.Value = 1.0;
                }
                else if (physicsParam.Name is "StrengthEstimatorOffsetStdDev")
                {
                    physicsParam.Value = 0.0;
                }
            }

            repository.Setup(r => r.GetPhysicsParams()).Returns(physicsParams);
            GameRecord gameRecord = new()
            {
                AwayTeamID = 1,
                HomeTeamID = 2,
                AwayTeam = awayTeam,
                HomeTeam = homeTeam,
                Stadium = homeStadium,
                KickoffTime = DateTimeOffset.Parse("2026-01-31T00:00:00Z"),
                GameType = GameType.RegularSeason
            };
            repository.Setup(r => r.GetNextUnplayedGame()).Returns(gameRecord);
            IReadOnlyList<InjuryRecovery> injuryRecoveries = [
                    new InjuryRecovery
                    {
                        TeamID = 1,
                        Strength = "PassingOffenseStrength",
                        StrengthDelta = -5.0
                    },
                    new InjuryRecovery
                    {
                        TeamID = 2,
                        Strength = "RunningDefenseStrength",
                        StrengthDelta = -3.0
                    }
                ];
            repository.Setup(r => r.GetInjuryRecoveriesForGame(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTimeOffset>()))
                .Returns(injuryRecoveries);
            repository.Setup(r => r.GetStadium(It.IsAny<int>())).Returns<int>(id => id == 1 ? awayStadium : homeStadium);
            repository.Setup(r => r.GetActiveRosterForTeam(It.IsAny<int>())).Returns([new PlayerRosterPosition
            {
                JerseyNumber = 37
            }]);

            var randomFactory = new Mock<IRandomFactory>();
            var random = new Mock<IRandom>();
            randomFactory.Setup(rf => rf.Create()).Returns(random.Object);
            random.Setup(r => r.SampleNormalDistribution(It.IsAny<double>(), It.IsAny<double>())).Returns<double, double>((mean, stddev) => mean);
            random.Setup(r => r.Chance(It.IsAny<double>())).Returns<double>(chance => chance >= 0.5);
            random.Setup(r => r.NextDouble()).Returns(0.5d);

            // Act
            var step = LoadGameStep.Run(TestHelpers.EmptySystemContext with
            {
                Environment = new SystemEnvironment
                {
                    FootballRepository = repository.Object,
                    RandomFactory = randomFactory.Object,
                    SummaryWriter = null!,
                    PlayerFactory = null!,
                    DebugContextWriter = null!,
                    EventBus = Mock.Of<IEventBus>()
                }
            });

            // Assert
            repository.Verify(r => r.SaveChanges(), Times.Exactly(2));
            repository.Verify(r => r.GetInjuryRecoveriesForGame(gameRecord.AwayTeamID, gameRecord.HomeTeamID, gameRecord.KickoffTime), Times.Once);
            Assert.All(injuryRecoveries, ir => Assert.True(ir.Recovered));
            Assert.Equal(gameRecord, step.Environment.CurrentGameRecord);
            Assert.NotNull(step.Environment.CurrentGameContext);
            Assert.Equal(SystemState.InGame, step.NextState);
            var gameContext = step.Environment.CurrentGameContext!;
            Assert.Equal(GameState.Start, gameContext.NextState);
            Assert.NotNull(gameContext.Environment);
            Assert.Equal(-40, gameContext.AwayTeamAcclimatedTemperature);
            Assert.Equal(40, gameContext.HomeTeamAcclimatedTemperature);
            Assert.Equal(GameTeam.Away, gameContext.TeamWithPossession);
            Assert.Equal(0, gameContext.PlayCountOnDrive);
            var gameEnvironment = gameContext.Environment;
            Assert.Equal(repository.Object, gameEnvironment.FootballRepository);
            Assert.Equal(physicsParams.Count, gameEnvironment.PhysicsParams.Count);
            Assert.NotNull(gameEnvironment.CurrentPlayContext);
            Assert.Equal(gameRecord, gameEnvironment.CurrentGameRecord);
            Assert.Equal(randomFactory.Object, gameEnvironment.RandomFactory);
            Assert.Null(gameEnvironment.DebugContextWriter);
            Assert.Single(gameEnvironment.AwayActiveRoster);
            Assert.Single(gameEnvironment.HomeActiveRoster);
            var playContext = gameEnvironment.CurrentPlayContext!;
            Assert.Equal(PlayEvaluationState.Start, playContext.NextState);
            Assert.Empty(playContext.StateHistory);
            Assert.Empty(playContext.AdditionalParameters);
            Assert.Equal(180d, playContext.BaseWindDirection);
            Assert.Equal(20d, playContext.BaseWindSpeed);
            Assert.Equal(40d, playContext.AirTemperature);
            Assert.Equal(GameTeam.Home, playContext.CoinFlipWinner);
            Assert.Equal(GameTeam.Away, playContext.TeamWithPossession);
            Assert.Equal(0, playContext.AwayScore);
            Assert.Equal(0, playContext.HomeScore);
            Assert.Equal(1, playContext.PeriodNumber);
            Assert.Equal(Constants.SecondsPerQuarter, playContext.SecondsLeftInPeriod);
            Assert.False(playContext.ClockRunning);
            Assert.Equal(3, playContext.AwayTimeoutsRemaining);
            Assert.Equal(3, playContext.HomeTimeoutsRemaining);
            Assert.NotNull(playContext.PlayInvolvement);
            Assert.Equal(65, playContext.LineOfScrimmage);
            Assert.Null(playContext.LineToGain);
            Assert.Equal(NextPlayKind.Kickoff, playContext.NextPlay);
            Assert.Equal(50, playContext.DriveStartingFieldPosition);
            Assert.Equal(1, playContext.DriveStartingPeriodNumber);
            Assert.Equal(Constants.SecondsPerQuarter, playContext.DriveStartingSecondsLeftInPeriod);
            Assert.Null(playContext.DriveResult);
            Assert.False(playContext.AwayScoredThisPlay);
            Assert.False(playContext.HomeScoredThisPlay);
            Assert.Equal(PossessionOnPlay.None, playContext.PossessionOnPlay);
            Assert.Null(playContext.TeamCallingTimeout);
            var playInvolvement = playContext.PlayInvolvement!;
            Assert.False(playInvolvement.InvolvesOffenseRun);
            Assert.False(playInvolvement.InvolvesDefenseRun);
            Assert.False(playInvolvement.InvolvesKick);
            Assert.False(playInvolvement.InvolvesOffensePass);
            Assert.Equal(0, playInvolvement.OffensivePlayersInvolved);
            Assert.Equal(0, playInvolvement.DefensivePlayersInvolved);
            Assert.NotNull(playContext.Environment);
            var playEnvironment = playContext.Environment;
            Assert.NotNull(playEnvironment.DecisionParameters);
            var decisionParameters = playEnvironment.DecisionParameters;
            Assert.NotNull(decisionParameters.Random);
            Assert.Equal(awayTeam, decisionParameters.AwayTeam);
            Assert.Equal(homeTeam, decisionParameters.HomeTeam);
            Assert.Equal(gameRecord.GameType, decisionParameters.GameType);
            TestHelpers.AssertStrengthsEqual(decisionParameters.AwayTeamActualStrengths, awayTeam);
            TestHelpers.AssertStrengthsEqual(decisionParameters.HomeTeamActualStrengths, homeTeam);

            // Based on how we set up the physics params and the random mock, the estimated strengths
            // should be exactly one half of what the actual strengths are
            TestHelpers.AssertEstimatedStrengthsEqual(decisionParameters.AwayTeamEstimateOfAway, awayTeam, 0.5);
            TestHelpers.AssertEstimatedStrengthsEqual(decisionParameters.AwayTeamEstimateOfHome, homeTeam, 0.5);
            TestHelpers.AssertEstimatedStrengthsEqual(decisionParameters.HomeTeamEstimateOfAway, awayTeam, 0.5);
            TestHelpers.AssertEstimatedStrengthsEqual(decisionParameters.HomeTeamEstimateOfHome, homeTeam, 0.5);
            Assert.Equal(physicsParams.ToDictionary(p => p.Name, p => p), playEnvironment.PhysicsParams);
        }

        // Run_GoesToPrepareForGameWhenNoUnplayedGames()
        // Run_ThrowsWhenMonthOutOfRange()
    }
}
