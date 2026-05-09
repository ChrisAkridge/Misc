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
using Moq;
using Celarix.JustForFun.FootballSimulator.Output;

namespace Celarix.JustForFun.FootballSimulator.Tests.Core
{
    public class PlayContextFuzzer
    {
        private readonly ITestOutputHelper output;
        private readonly IReadOnlyDictionary<string, PhysicsParam> physicsParams = SeedData.ParamSeedData()
            .ToDictionary(p => p.Name, p => p);

        public PlayContextFuzzer(ITestOutputHelper output)
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
            var random = new global::System.Random(seed);
            var playStatesForIteration = new List<PlayContext>();

            const int iterations = 1000;
            for (var i = 0; i < iterations; i++)
            {
                if (i == 955)
                {
                    Debugger.Break();
                }

                var gameState = GenerateRandomGameState(random);
                var decisionParameters = gameState.Environment!.DecisionParameters;
                playStatesForIteration.Add(gameState);

                Log.Information($"ITERATION {i} Before: " + gameState.GetDescription(decisionParameters));

                try
                {
                    do
                    {
                        gameState = MoveNext(gameState);
                        AssertPropertyBasedTests(gameState);
                        playStatesForIteration.Add(gameState);
                    } while (gameState.NextState != PlayEvaluationState.PlayEvaluationComplete);

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
        private PlayContext GenerateRandomGameState(global::System.Random random)
        {
            NextPlayKind nextPlay = GetRandomEnumValue<NextPlayKind>(random);
            PlayEvaluationState validStartStateForNextPlay = nextPlay switch
            {
                NextPlayKind.Kickoff => PlayEvaluationState.KickoffDecision,
                NextPlayKind.FirstDown or NextPlayKind.SecondDown or NextPlayKind.ThirdDown or NextPlayKind.FourthDown => PlayEvaluationState.MainGameDecision,
                NextPlayKind.ConversionAttempt => PlayEvaluationState.TouchdownDecision,
                NextPlayKind.FreeKick => PlayEvaluationState.FreeKickDecision,
                _ => throw new InvalidOperationException($"Unhandled next play kind {nextPlay}.")
            };

            GameTeam teamWithPossession = GetRandomEnumValue<GameTeam>(random);
            int lineOfScrimmage = validStartStateForNextPlay switch
            {
                PlayEvaluationState.KickoffDecision => teamWithPossession == GameTeam.Home ? 35 : 65,
                PlayEvaluationState.FreeKickDecision => teamWithPossession == GameTeam.Home ? 20 : 80,
                PlayEvaluationState.TouchdownDecision => teamWithPossession == GameTeam.Home ? 85 : 15,
                PlayEvaluationState.MainGameDecision => random.Next(1, 100),
                _ => throw new InvalidOperationException($"Unhandled start state {validStartStateForNextPlay}.")
            };
            var movingToHigherYardNumber = teamWithPossession == GameTeam.Home;
            var distanceToEndZone = movingToHigherYardNumber ? 100 - lineOfScrimmage : lineOfScrimmage;
            int? yardsToGain = validStartStateForNextPlay == PlayEvaluationState.MainGameDecision
                ? random.Next(1, distanceToEndZone + 1)
                : null;
            int? lineToGain = null;
            if (yardsToGain == distanceToEndZone)
            {
                // If we're at the end zone, line to gain is null
                lineToGain = null;
            }
            else if (validStartStateForNextPlay == PlayEvaluationState.MainGameDecision && yardsToGain.HasValue)
            {
                lineToGain = Helpers.AddYardsForTeam(lineOfScrimmage, yardsToGain.Value, teamWithPossession).Round();
            }

            int periodNumber = random.Next(1, 9);
            int secondsLeftInPeriod = random.Next(0, 900);
            return new PlayContext(
                Version: 0L,
                NextState: validStartStateForNextPlay,
                AdditionalParameters: [],
                StateHistory: [],
                Environment: new PlayEnvironment
                {
                    DecisionParameters = GenerateRandomGameDecisionParameters(random.Next(), random),
                    PhysicsParams = physicsParams,
                    EventBus = Mock.Of<IEventBus>()
                },
                BaseWindDirection: random.NextDouble() * 360d,
                BaseWindSpeed: random.SampleNormalDistribution(5d, 2d),
                AirTemperature: random.SampleNormalDistribution(60d, 10d),
                CoinFlipWinner: random.NextDouble() < 0.5d ? GameTeam.Home : GameTeam.Away,
                TeamWithPossession: teamWithPossession,
                AwayScore: random.Next(0, 50),
                HomeScore: random.Next(0, 50),
                PeriodNumber: periodNumber,
                SecondsLeftInPeriod: secondsLeftInPeriod,
                ClockRunning: random.NextDouble() < 0.5d,
                HomeTimeoutsRemaining: random.Next(0, 4),
                AwayTimeoutsRemaining: random.Next(0, 4),
                PlayInvolvement: new PlayInvolvement(
                    InvolvesOffenseRun: false,
                    InvolvesOffensePass: false,
                    InvolvesKick: false,
                    InvolvesDefenseRun: false,
                    OffensivePlayersInvolved: 0,
                    DefensivePlayersInvolved: 0),
                LineOfScrimmage: lineOfScrimmage,
                LineToGain: lineToGain,
                NextPlay: nextPlay,
                DriveStartingFieldPosition: lineOfScrimmage,
                DriveStartingPeriodNumber: periodNumber,
                DriveStartingSecondsLeftInPeriod: secondsLeftInPeriod,
                DriveResult: null,
                LastPlayDescriptionTemplate: "",
                AwayScoredThisPlay: false,
                HomeScoredThisPlay: false,
                PossessionOnPlay: (PossessionOnPlay)random.Next(0, 3),
                TeamCallingTimeout: random.NextDouble() < 0.03d
                    ? global::Celarix.JustForFun.FootballSimulator.Tests.Core.PlayContextFuzzer.GetRandomEnumValue<GameTeam>(random)
                    : null);
        }

        private static GameDecisionParameters GenerateRandomGameDecisionParameters(int randomSeed, global::System.Random random)
        {
            var awayTeam = new Team
            {
                CityName = "Redmond",
                TeamName = "Redmond Passing Tests",
                Abbreviation = "RPT",
                Disposition = GetRandomEnumValue<TeamDisposition>(random)
            };
            var homeTeam = new Team
            {
                CityName = "London",
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

        private static TeamStrengthSet GenerateRandomTeamStrengthSet(global::System.Random random, bool isEstimate, GameTeam forTeam, GameTeam byTeam)
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

        private static TEnum GetRandomEnumValue<TEnum>(global::System.Random random, params TEnum[] except) where TEnum : Enum
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
        private static PlayContext MoveNext(PlayContext currentState)
        {
            var nextState = currentState.NextState switch
            {
                PlayEvaluationState.Start => throw new InvalidOperationException("uh... shouldn't be here yet, I think?"),
                PlayEvaluationState.KickoffDecision => KickoffDecision.Run(currentState),
                PlayEvaluationState.FreeKickDecision => FreeKickDecision.Run(currentState),
                PlayEvaluationState.SignalFairCatchDecision => SignalFairCatchDecision.Run(currentState),
                PlayEvaluationState.TouchdownDecision => TouchdownDecision.Run(currentState),
                PlayEvaluationState.MainGameDecision => MainGameDecision.Run(currentState),
                PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision => ReturnFumbledOrInterceptedBallDecision.Run(currentState),
                PlayEvaluationState.NormalKickoffOutcome => NormalKickoffOutcome.Run(currentState),
                PlayEvaluationState.ReturnableKickOutcome => ReturnableKickOutcome.Run(currentState),
                PlayEvaluationState.FumbledLiveBallOutcome => FumbledLiveBallOutcome.Run(currentState),
                PlayEvaluationState.PuntOutcome => PuntOutcome.Run(currentState),
                PlayEvaluationState.ReturnablePuntOutcome => ReturnablePuntOutcome.Run(currentState),
                PlayEvaluationState.KickOrPuntReturnOutcome => KickOrPuntReturnOutcome.Run(currentState),
                PlayEvaluationState.FumbleOrInterceptionReturnOutcome => FumbleOrInterceptionReturnOutcome.Run(currentState),
                PlayEvaluationState.OnsideKickAttemptOutcome => OnsideKickAttemptOutcome.Run(currentState),
                PlayEvaluationState.FieldGoalsAndExtraPointAttemptOutcome => FieldGoalAttemptOutcome.Run(currentState),
                PlayEvaluationState.TwoPointConversionAttemptOutcome => TwoPointConversionAttemptOutcome.Run(currentState),
                PlayEvaluationState.StandardRushingPlayOutcome => StandardRushingPlayOutcome.Run(currentState),
                PlayEvaluationState.StandardShortPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, PassAttemptDistance.Short),
                PlayEvaluationState.StandardMediumPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, PassAttemptDistance.Medium),
                PlayEvaluationState.StandardLongPassingPlayOutcome => StandardPassingPlayOutcome.Run(currentState, PassAttemptDistance.Long),
                PlayEvaluationState.HailMaryOutcome => HailMaryOutcome.Run(currentState),
                PlayEvaluationState.QBSneakOutcome => QBSneakOutcome.Run(currentState),
                PlayEvaluationState.FakePuntOutcome => FakePuntOrFieldGoalOutcome.Run(currentState),
                PlayEvaluationState.FakeFieldGoalOutcome => FakePuntOrFieldGoalOutcome.Run(currentState),
                PlayEvaluationState.VictoryFormationOutcome => VictoryFormationOutcome.Run(currentState),
                _ => throw new InvalidOperationException($"Unhandled gameplay next state {currentState.NextState}.")
            };

            nextState = nextState with
            {
                Version = nextState.Version + 1
            };

            return nextState;
        }
        #endregion

        private static void AssertPropertyBasedTests(PlayContext state)
        {
            Assert.True(state.Version >= 0L);
            Assert.True(Enum.IsDefined(state.NextState));
            Assert.NotEqual(PlayEvaluationState.Start, state.NextState);
            Assert.True(state.AdditionalParameters.Count == 0
                || state.AdditionalParameters.All(p => p.Key is "IsFakePlay"
                or "WasIntercepted"
                or "KickActualYard"
                or "IsConversionAttempt"));
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
            Assert.Equal(PossessionOnPlay.None, state.PossessionOnPlay & ~PossessionOnPlay.BothTeams);
            Assert.True(state.TeamCallingTimeout is null
                || Enum.IsDefined(state.TeamCallingTimeout.Value));
        }

        #region Next-Play Assertions
        private static void AssertForNextPlay(List<PlayContext> allStates)
        {
            var nextPlay = allStates.First().NextPlay;
            if (nextPlay == NextPlayKind.Kickoff)
            {
                AssertForKickoff(allStates);
            }
            else if (nextPlay == NextPlayKind.FirstDown || nextPlay == NextPlayKind.SecondDown || nextPlay == NextPlayKind.ThirdDown || nextPlay == NextPlayKind.FourthDown)
            {
                AssertForFirstDownOrDownReset(allStates);
            }
            else if (nextPlay == NextPlayKind.ConversionAttempt)
            {
                AssertForConversionAttempt(allStates);
            }
            else if (nextPlay == NextPlayKind.FreeKick)
            {
                // The outcomes here are pretty much the same as a kickoff.
                AssertForKickoff(allStates);
            }
        }

        private static void AssertForKickoff(List<PlayContext> allGameStates)
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
            var fumbleOccurred = allGameStates.Any(gs => gs.NextState == PlayEvaluationState.FumbledLiveBallOutcome);
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

        private static void AssertForFirstDownOrDownReset(List<PlayContext> allGameStates)
        {
            // A first down or down reset can result in:
            // Either team scoring a touchdown
            // The offense gaining at least 10 yards from the line of scrimmage for a first down
            // The offense losing yards and not getting a first down
            // A turnover (on downs, fumble recovery, or interception) and either team potentially scoring a touchdown on the turnover
            // A turnover and the other team getting first down from where the ball was recovered
            var initialState = allGameStates.First();
            var resultingState = allGameStates.Last();
            var offenseTeam = initialState.TeamWithPossession;
            var touchdownOccurred = resultingState.NextPlay == NextPlayKind.ConversionAttempt;
            if (touchdownOccurred)
            {
                Assert.True(resultingState.AwayScore == initialState.AwayScore + 6
                    || resultingState.HomeScore == initialState.HomeScore + 6);
            }

            var isFirstDown = resultingState.NextPlay == NextPlayKind.FirstDown;
            if (isFirstDown)
            {
                var offensePossessionAtEnd = resultingState.TeamWithPossession == offenseTeam;
                if (offensePossessionAtEnd)
                {
                    var multipleTurnoversOccured = allGameStates
                        .Count(gs => gs.NextState == PlayEvaluationState.FumbledLiveBallOutcome) > 1
                        || allGameStates.Count(gs => gs.NextState == PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision) > 1;
                    var timeoutCalled = allGameStates.Any(gs => gs.TeamCallingTimeout.HasValue);
                    var muffedPuntFumbledByReceivingTeam = allGameStates.Any(gs => gs.NextState == PlayEvaluationState.PuntOutcome);

                    if (!multipleTurnoversOccured
                        && !timeoutCalled
                        && !muffedPuntFumbledByReceivingTeam)
                    {
                        var yardsGained = resultingState.DistanceForPossessingTeam(initialState.LineOfScrimmage, resultingState.LineOfScrimmage);
                        var yardsToGo = initialState.LineToGain.HasValue
                            ? resultingState.DistanceForPossessingTeam(initialState.LineOfScrimmage, initialState.LineToGain.Value)
                            : 0;
                        Assert.True(yardsGained >= yardsToGo);
                    }
                }
                else
                {
                    Assert.NotEqual(resultingState.TeamWithPossession, offenseTeam);
                }
            }

            AssertLineToGainPosition(resultingState);
        }

        private static void AssertForConversionAttempt(List<PlayContext> allGameStates)
        {
            // A conversion attempt can result in:
            // - No score change for either team
            // - An extra point for the offense
            // - A two-point conversion for the offense
            // - A two-point conversion for the defense if the ball is turned over and returned
            // - A one-point safety for the offense if the ball is turned over but the defense is downed in their own end zone
            // - A one-point safety for the defense if the offense is downed in their own endzone somehow
            // - Both teams cannot score
            // - The next play is always kickoff
            var initialState = allGameStates.First();
            var resultingState = allGameStates.Last();
            var awayScoreDifference = resultingState.GetScoreForTeam(GameTeam.Away) - initialState.GetScoreForTeam(GameTeam.Away);
            var homeScoreDifference = resultingState.GetScoreForTeam(GameTeam.Home) - initialState.GetScoreForTeam(GameTeam.Home);
            var turnoverOccurred = allGameStates.Any(gs => gs.NextState == PlayEvaluationState.FumbledLiveBallOutcome)
                || allGameStates.Any(gs => gs.NextState == PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision);
            var offenseHasPossessionAtEnd = resultingState.TeamWithPossession == initialState.TeamWithPossession;

            Assert.Equal(NextPlayKind.Kickoff, resultingState.NextPlay);

            if (homeScoreDifference > 0)
            {
                Assert.True(homeScoreDifference == 1 || homeScoreDifference == 2);
                Assert.Equal(0, awayScoreDifference);
            }
            else if (awayScoreDifference > 0)
            {
                Assert.True(awayScoreDifference == 1 || awayScoreDifference == 2);
                Assert.Equal(0, homeScoreDifference);
            }
        }

        private static void AssertLineToGainPosition(PlayContext playContext)
        {
            // Assertion passes if:
            // - Next play is first, second, third or fourth down,
            // - AND line to gain is ahead of the line of scrimmage
            //   (away possession means LtG < LoS, home possession means LtG > LoS)
            if (playContext.NextPlay == NextPlayKind.FirstDown
                || playContext.NextPlay == NextPlayKind.SecondDown
                || playContext.NextPlay == NextPlayKind.ThirdDown
                || playContext.NextPlay == NextPlayKind.FourthDown)
            {
                if (playContext.LineToGain is null)
                {
                    // Goal to go! Assertion passes.
                    return;
                }

                if (playContext.TeamWithPossession == GameTeam.Away)
                {
                    Assert.True(playContext.LineToGain < playContext.LineOfScrimmage);
                }
                else
                {
                    Assert.True(playContext.LineToGain > playContext.LineOfScrimmage);
                }
            }
        }
        #endregion
    }
}
