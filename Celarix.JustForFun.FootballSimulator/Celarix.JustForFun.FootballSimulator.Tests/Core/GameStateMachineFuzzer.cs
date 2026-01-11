using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Abstractions;
using Xunit.Sdk;
using System.IO;
using System.Diagnostics;
using Celarix.JustForFun.FootballSimulator.Core;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core
{
    public class GameStateMachineFuzzer
    {
        private readonly ITestOutputHelper output;

        public GameStateMachineFuzzer(ITestOutputHelper output)
        {
            this.output = output;

            Serilog.Debugging.SelfLog.Enable(x => Debug.WriteLine(x)); // Or x => Debug.WriteLine(x)

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "fuzzerOutput.txt"), rollingInterval: RollingInterval.Hour)
            .CreateLogger();
        }

        [Fact]
        public void RunFuzzer()
        {
            Log.Information("===== STARTING NEW FUZZER RUN =====");

            //var seed = Environment.TickCount;
            var seed = 279259703;
            Log.Information($"Using seed: {seed}");

            // Fixed-seed random: used for reproducing specific test cases
            var random = new System.Random(seed);
            var playStatesForIteration = new List<GameState>();

            const int iterations = 1000;
            for (var i = 0; i < iterations; i++)
            {
                var gameState = GenerateRandomGameState(random);
                var decisionParameters = GenerateRandomGameDecisionParameters(random.Next(), random);
                var physicsParams = GeneratePhysicsParams(random);

                Log.Information($"ITERATION {i} Before: " + gameState.GetDescription(decisionParameters));

                try
                {
                    do
                    {
                        gameState = MoveNext(gameState, decisionParameters, physicsParams);
                        AssertPropertyBasedTests(gameState);
                        playStatesForIteration.Add(gameState);
                    } while (gameState.NextState != GameplayNextState.PlayEvaluationComplete);

                    if (i == 2 && gameState.Version == 1)
                    {
                        Debugger.Break();
                    }

                    Log.Information($"ITERATION {i} After: " + gameState.GetDescription(decisionParameters));

                    AssertForNextPlay(playStatesForIteration);
                    playStatesForIteration.Clear();
                }
                catch (XunitException ex)
                {
                    Log.Error(ex, $"Assertion failed on iteration {i} version {gameState.Version} (zero-indexed, total iterations {iterations})");
                    Log.CloseAndFlush();
                    throw;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Exception encountered on iteration {i} version {gameState.Version} (zero-indexed, total iterations {iterations})");
                    Log.CloseAndFlush();
                    throw;
                }
            }
            Log.CloseAndFlush();
        }

        #region Random Generators
        private GameState GenerateRandomGameState(System.Random random)
        {
            NextPlayKind nextPlay = GetRandomEnumValue<NextPlayKind>(random);
            GameplayNextState validStartStateForNextPlay = nextPlay switch
            {
                NextPlayKind.Kickoff => GameplayNextState.KickoffDecision,
                NextPlayKind.FirstDown or NextPlayKind.SecondDown or NextPlayKind.ThirdDown or NextPlayKind.FourthDown => GameplayNextState.MainGameDecision,
                NextPlayKind.ConversionAttempt => GameplayNextState.TouchdownDecision,
                NextPlayKind.FreeKick => GameplayNextState.FreeKickDecision,
                _ => throw new InvalidOperationException($"Unhandled next play kind {nextPlay}.")
            };

            GameTeam teamWithPossession = GetRandomEnumValue<GameTeam>(random);
            int lineOfScrimmage = validStartStateForNextPlay switch
            {
                GameplayNextState.KickoffDecision => teamWithPossession == GameTeam.Home ? 35 : 65,
                GameplayNextState.FreeKickDecision => teamWithPossession == GameTeam.Home ? 20 : 80,
                GameplayNextState.TouchdownDecision => teamWithPossession == GameTeam.Home ? 85 : 15,
                GameplayNextState.MainGameDecision => random.Next(1, 100),
                _ => throw new InvalidOperationException($"Unhandled start state {validStartStateForNextPlay}.")
            };
            var movingToHigherYardNumber = teamWithPossession == GameTeam.Home;
            var distanceToEndZone = movingToHigherYardNumber ? 100 - lineOfScrimmage : lineOfScrimmage;
            int? yardsToGain = validStartStateForNextPlay == GameplayNextState.MainGameDecision
                ? random.Next(1, distanceToEndZone + 1)
                : null;
            int? lineToGain = null;
            if (yardsToGain == distanceToEndZone)
            {
                // If we're at the end zone, line to gain is null
                lineToGain = null;
            }
            else if (validStartStateForNextPlay == GameplayNextState.MainGameDecision && yardsToGain.HasValue)
            {
                lineToGain = Helpers.AddYardsForTeam(lineOfScrimmage, yardsToGain.Value, teamWithPossession).Round();
            }

            return new GameState(
                Version: 0L,
                NextState: validStartStateForNextPlay,
                AdditionalParameters: [],
                StateHistory: [],
                BaseWindDirection: random.NextDouble() * 360d,
                BaseWindSpeed: random.SampleNormalDistribution(5d, 2d),
                AirTemperature: random.SampleNormalDistribution(60d, 10d),
                TeamWithPossession: teamWithPossession,
                AwayScore: random.Next(0, 50),
                HomeScore: random.Next(0, 50),
                PeriodNumber: random.Next(1, 9),
                SecondsLeftInPeriod: random.Next(0, 900),
                ClockRunning: random.NextDouble() < 0.5d,
                HomeTimeoutsRemaining: random.Next(0, 4),
                AwayTimeoutsRemaining: random.Next(0, 4),
                LineOfScrimmage: lineOfScrimmage,
                LineToGain: lineToGain,
                NextPlay: nextPlay,
                LastPlayDescriptionTemplate: "",
                PossessionOnPlay: (PossessionOnPlay)random.Next(0, 3),
                TeamCallingTimeout: random.NextDouble() < 0.03d
                    ? GetRandomEnumValue<GameTeam>(random)
                    : null);
        }

        private GameDecisionParameters GenerateRandomGameDecisionParameters(int randomSeed, System.Random random)
        {
            var awayTeam = new Team
            {
                TeamName = "Redmond Passing Tests",
                Abbreviation = "RPT",
                Disposition = GetRandomEnumValue<TeamDisposition>(random)
            };
            var homeTeam = new Team
            {
                TeamName = "London Asserters",
                Abbreviation = "LAS",
                Disposition = GetRandomEnumValue<TeamDisposition>(random)
            };
            return new GameDecisionParameters
            {
                Random = new RandomWrapper(randomSeed),
                AwayTeam = awayTeam,
                HomeTeam = homeTeam,
                AwayTeamActualStrengths = GenerateRandomTeamStrengthSet(random, false, GameTeam.Away, GameTeam.Away),
                HomeTeamActualStrengths = GenerateRandomTeamStrengthSet(random, false, GameTeam.Home, GameTeam.Home),
                AwayTeamEstimateOfAway = GenerateRandomTeamStrengthSet(random, true, GameTeam.Away, GameTeam.Away),
                AwayTeamEstimateOfHome = GenerateRandomTeamStrengthSet(random, true, GameTeam.Away, GameTeam.Home),
                HomeTeamEstimateOfAway = GenerateRandomTeamStrengthSet(random, true, GameTeam.Home, GameTeam.Away),
                HomeTeamEstimateOfHome = GenerateRandomTeamStrengthSet(random, true, GameTeam.Home, GameTeam.Home),
                GameType = GetRandomEnumValue<GameType>(random)
            };
        }

        private TeamStrengthSet GenerateRandomTeamStrengthSet(System.Random random, bool isEstimate, GameTeam forTeam, GameTeam byTeam)
        {
            return new TeamStrengthSet
            {
                IsEstimate = isEstimate,
                Team = forTeam,
                StrengthSetKind = forTeam == GameTeam.Home
                    ? (byTeam == GameTeam.Home
                        ? StrengthSetKind.TeamOfItself
                        : StrengthSetKind.TeamOfOpponent)
                    : (byTeam == GameTeam.Away
                        ? StrengthSetKind.TeamOfItself
                        : StrengthSetKind.TeamOfOpponent),
                RunningOffenseStrength = random.Next(0, 500),
                RunningDefenseStrength = random.Next(0, 500),
                PassingOffenseStrength = random.Next(0, 500),
                PassingDefenseStrength = random.Next(0, 500),
                OffensiveLineStrength = random.Next(0, 500),
                DefensiveLineStrength = random.Next(0, 500),
                KickingStrength = random.Next(0, 500),
                FieldGoalStrength = random.Next(0, 500),
                KickReturnStrength = random.Next(0, 500),
                KickDefenseStrength = random.Next(0, 500),
                ClockManagementStrength = random.Next(0, 500),
            };
        }

        private IReadOnlyDictionary<string, PhysicsParam> GeneratePhysicsParams(System.Random random)
        {
            var physicsParams = SeedData.ParamSeemData()
                .ToDictionary(p => p.Name, p => p);

            foreach (var param in physicsParams.Values)
            {
                var multiplier = random.SampleNormalDistribution(1d, 0.1d);
                param.Value *= multiplier;
            }

            return physicsParams;
        }

        private TEnum GetRandomEnumValue<TEnum>(System.Random random, params TEnum[] except) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum));
            TEnum selectedValue;

            do
            {
                selectedValue = (TEnum)values.GetValue(random.Next(values.Length))!;
            } while (except is not null && selectedValue!.Equals(except));

            return selectedValue;
        }
        #endregion

        #region Main Loop
        private GameState MoveNext(GameState currentState, GameDecisionParameters currentParameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var nextState = currentState.NextState switch
            {
                GameplayNextState.Start => throw new InvalidOperationException("uh... shouldn't be here yet, I think?"),
                GameplayNextState.KickoffDecision => KickoffDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FreeKickDecision => FreeKickDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.SignalFairCatchDecision => SignalFairCatchDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.TouchdownDecision => TouchdownDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.MainGameDecision => MainGameDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.ReturnFumbledOrInterceptedBallDecision => ReturnFumbledOrInterceptedBallDecision.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.NormalKickoffOutcome => NormalKickoffOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.ReturnableKickOutcome => ReturnableKickOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FumbledLiveBallOutcome => FumbledLiveBallOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.PuntOutcome => PuntOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.ReturnablePuntOutcome => ReturnablePuntOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.KickOrPuntReturnOutcome => KickOrPuntReturnOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FumbleOrInterceptionReturnOutcome => FumbleOrInterceptionReturnOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.OnsideKickAttemptOutcome => OnsideKickAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FieldGoalsAndExtraPointAttemptOutcome => FieldGoalAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.TwoPointConversionAttemptOutcome => TwoPointConversionAttemptOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.StandardRushingPlayOutcome => StandardRushingPlayOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.StandardShortPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Short),
                GameplayNextState.StandardMediumPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Medium),
                GameplayNextState.StandardLongPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, currentParameters, physicsParams, PassAttemptDistance.Long),
                GameplayNextState.HailMaryOutcome => HailMaryOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.QBSneakOutcome => QBSneakOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FakePuntOutcome => FakePuntOrFieldGoalOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.FakeFieldGoalOutcome => FakePuntOrFieldGoalOutcome.Run(currentState, currentParameters, physicsParams),
                GameplayNextState.VictoryFormationOutcome => VictoryFormationOutcome.Run(currentState, currentParameters, physicsParams),
                _ => throw new InvalidOperationException($"Unhandled gameplay next state {currentState.NextState}.")
            };

            nextState = nextState with
            {
                Version = nextState.Version + 1
            };

            return nextState;
        }
        #endregion

        private static void AssertPropertyBasedTests(GameState state)
        {
            Assert.True(state.Version >= 0L);
            Assert.True(Enum.IsDefined(state.NextState));
            Assert.NotEqual(GameplayNextState.Start, state.NextState);
            Assert.True(state.AdditionalParameters.Count == 0
                || state.AdditionalParameters.All(p => p.Key is "IsFakePlay" or "WasIntercepted" or "KickActualYard"));
            Assert.True(state.StateHistory.Count > 0);
            Assert.InRange(state.BaseWindDirection, 0d, 360d);
            Assert.True(Enum.IsDefined(state.TeamWithPossession));
            Assert.True(state.AwayScore >= 0);
            Assert.True(state.HomeScore >= 0);
            Assert.True(state.PeriodNumber >= 1);
            Assert.InRange(state.SecondsLeftInPeriod, 0, 900);
            Assert.InRange(state.HomeTimeoutsRemaining, 0, 3);
            Assert.InRange(state.AwayTimeoutsRemaining, 0, 3);
            Assert.InRange(state.LineOfScrimmage, -10, 110);
            Assert.True(state.LineToGain is null || (state.LineToGain >= 0 && state.LineToGain <= 100));
            Assert.True(Enum.IsDefined(state.NextPlay));
            Assert.True((state.PossessionOnPlay & ~PossessionOnPlay.BothTeams) == 0);
            Assert.True(state.TeamCallingTimeout is null
                || Enum.IsDefined(state.TeamCallingTimeout.Value));
        }

        #region Next-Play Assertions
        private void AssertForNextPlay(List<GameState> allStates)
        {
            var nextPlay = allStates.First().NextPlay;
            if (nextPlay == NextPlayKind.Kickoff)
            {
                AssertForKickoff(allStates);
            }
        }

        private void AssertForKickoff(List<GameState> allGameStates)
        {
            // A kickoff can result in:
            // Either team scoring either a safety or a touchdown
            // A first down for the receiving team from anywhere on the field
            // A first down for the kicking team but only at least 10 yards ahead of the kickoff location
            // A first down for either team anywhere on the field but only if a fumble occurs

            var initialState = allGameStates.First();
            var resultingState = allGameStates.Last();
            var kickingTeam = initialState.TeamWithPossession;
            var receivingTeam = kickingTeam.Opponent();
            var fumbleOccurred = allGameStates.Any(gs => gs.NextState == GameplayNextState.FumbledLiveBallOutcome);
            var touchdownOccurred = resultingState.NextPlay == NextPlayKind.ConversionAttempt;
            var safetyOccurred = resultingState.AwayScore == initialState.AwayScore + 2
                || resultingState.HomeScore == initialState.HomeScore + 2;

            if (touchdownOccurred)
            {
                Assert.True(resultingState.AwayScore == initialState.AwayScore + 6
                    || resultingState.HomeScore == initialState.HomeScore + 6);
            }
            else if (safetyOccurred)
            {
                Assert.Equal(NextPlayKind.FreeKick, resultingState.NextPlay);
            }
            else
            {
                Assert.Equal(NextPlayKind.FirstDown, resultingState.NextPlay);
                if (resultingState.TeamWithPossession != receivingTeam)
                {
                    if (!fumbleOccurred)
                    {
                        var kickoffYardLine = initialState.LineOfScrimmage;
                        var resultingYardLine = resultingState.LineOfScrimmage;
                        var yardsGained = resultingState.DistanceForPossessingTeam(kickoffYardLine, resultingYardLine);
                        Assert.True(yardsGained >= 10);
                    }
                }
            }
        }
        #endregion
    }
}
