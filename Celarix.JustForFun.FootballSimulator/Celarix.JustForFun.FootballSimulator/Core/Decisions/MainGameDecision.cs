using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class MainGameDecision
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            
            GameTeam self = priorState.TeamWithPossession;
            GameTeam opponent = self.Opponent();

            // This... this is where the Ultra-Insane teams REALLY shine.
            var selfDisposition = parameters.GetDispositionForTeam(self);
            if (selfDisposition == TeamDisposition.UltraInsane)
            {
                Log.Verbose("MainGameDecision: Ultra-insane team disposition, ALWAYS Hail Mary!");
                for (var i = 0; i < 10; i++)
                {
                    Log.Verbose("Hail, Mary, full of grace, the Lord is with thee. Blessed art thou amongst women and blessed is the fruit of thy womb, Jesus. Holy Mary, Mother of God, pray for us sinners, now and at the hour of our death. Amen.");
                }
                return priorState.WithNextState(GameplayNextState.HailMaryOutcome);
            }

            var isFakePlay = priorState.GetAdditionalParameterOrDefault<bool?>("IsFakePlay") == true;

            var clockDisposition = ClockDispositionFunction.Get(priorState, parameters, physicsParams);
            var selfEstimateOfSelf = parameters.GetEstimateOfTeamByTeam(self, self);
            var selfEstimateOfOpponent = parameters.GetEstimateOfTeamByTeam(self, opponent);
            var selfEstimatedAverage = selfEstimateOfSelf.OverallAverageStrength;
            var selfEstimatedOpponentAverage = selfEstimateOfOpponent.OverallAverageStrength;
            var estimatedStrengthRatio = selfEstimatedAverage / selfEstimatedOpponentAverage;
            var adjustedPassingEstimate = selfEstimateOfSelf.PassingOffenseStrength;

            if (clockDisposition == ClockDisposition.TwoMinuteDrill)
            {
                if (priorState.ClockRunning)
                {
                    if (priorState.NextPlay == NextPlayKind.FourthDown)
                    {
                        Log.Verbose("MainGameDecision: Clock running during two-minute drill on fourth down, running play to avoid turnover on downs.");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }

                    var lineOfScrimmageTeamYard = priorState.InternalYardToTeamYard(priorState.LineOfScrimmage);
                    var spikeBallYardLoss = physicsParams["SpikeBallYardLoss"].Value;
                    if (lineOfScrimmageTeamYard.Team == self && lineOfScrimmageTeamYard.TeamYard <= spikeBallYardLoss)
                    {
                        Log.Verbose("MainGameDecision: Clock running during two-minute drill, but spiking the ball would result in a safety. Running play instead.");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }

                    var selfTimeoutsRemaining = priorState.TimeoutsRemainingForTeam(self);
                    if (selfTimeoutsRemaining > 1)
                    {
                        Log.Verbose("MainGameDecision: Clock running during two-minute drill, calling a timeout.");
                        return priorState.TakeTimeout(self);
                    }

                    Log.Verbose("MainGameDecision: Spiking ball to stop the clock!");
                    return PlayerDownedFunction.Get(priorState, parameters, physicsParams, priorState.LineOfScrimmage, -2,
                        priorState.NextPlay == NextPlayKind.ConversionAttempt ? EndzoneBehavior.ConversionAttempt : EndzoneBehavior.StandardGameplay,
                        priorState.NextPlay == NextPlayKind.ConversionAttempt ? self : null);
                }
                else
                {
                    var desirabilityMultiplier = physicsParams["TwoMinuteDrillPassDesirabilityMultiplier"].Value;
                    adjustedPassingEstimate *= desirabilityMultiplier;
                }

                if (priorState.NextPlay == NextPlayKind.FourthDown)
                {
                    var desireToGoForIt = 1.0;
                    var desireToNotGoForIt = 1.0;
                    var lineOfScrimmageTeamYard = priorState.InternalYardToTeamYard(priorState.LineOfScrimmage);
                    var fieldGoalRangeYard = physicsParams["FieldGoalRangeYard"].Value.Round();
                    bool inFieldGoalRange = lineOfScrimmageTeamYard.Team == opponent
                        && lineOfScrimmageTeamYard.TeamYard <= fieldGoalRangeYard;
                    if (inFieldGoalRange)
                    {
                        var undesirabilityMultiplier = physicsParams["FourthDownLongFieldUndesirabilityMultiplier"].Value;
                        desireToNotGoForIt *= undesirabilityMultiplier;
                    }

                    var desirabilityThresholdAlpha = physicsParams["FourthDownTrailingNearEndgamePointThreshold"].Value.Round();
                    var desirabilityThresholdBeta = physicsParams["FourthDownTrailingNearEndgameRemainingTimeThreshold"].Value;
                    var desirabilityMultiplier = physicsParams["FourthDownTrailingNearEndgameDesirabilityMultiplier"].Value;
                    var scoreDifference = priorState.GetScoreDifferenceForTeam(self);
                    if (scoreDifference > desirabilityThresholdAlpha && priorState.TotalSecondsLeftInGame() < desirabilityThresholdBeta)
                    {
                        desireToGoForIt *= desirabilityMultiplier;
                    }

                    var desirabilityThresholdGamma = physicsParams["FourthDownDistanceToGoThreshold"].Value;
                    desirabilityMultiplier = physicsParams["FourthDownDistanceToGoDesirabilityMultiplier"].Value;
                    var distanceToFirstDown = priorState.DistanceToGo();
                    if (!distanceToFirstDown.HasValue)
                    {
                        throw new InvalidOperationException("Distance to first down is null on fourth down.");
                    }
                    if (distanceToFirstDown.Value < desirabilityThresholdGamma)
                    {
                        desireToGoForIt *= desirabilityMultiplier;
                    }

                    var desirabilityThresholdDelta = physicsParams["FourthDownHistoricalRateThreshold"].Value;
                    desirabilityMultiplier = physicsParams["FourthDownHistoricalRateDesirabilityMultiplier"].Value;
                    if (parameters.GetFourthDownConversionRate(self) > desirabilityThresholdDelta)
                    {
                        desireToGoForIt *= desirabilityMultiplier;
                    }

                    var ratio = desireToGoForIt / (desireToGoForIt + desireToNotGoForIt);
                    var goForIt = parameters.Random.Chance(ratio);
                    if (goForIt)
                    {
                        var qbSneakDistance = physicsParams["FourthDownQBSneakDistanceThreshold"].Value;
                        if (distanceToFirstDown <= qbSneakDistance)
                        {
                            Log.Verbose("MainGameDecision: Going for it on fourth down with a QB sneak!");
                            return priorState.WithNextState(GameplayNextState.QBSneakOutcome);
                        }
                        Log.Verbose("MainGameDecision: Going for it on fourth down with a standard play!");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }

                    if (CanAttemptFieldGoal(priorState) && !isFakePlay && inFieldGoalRange)
                    {
                        var fakeFieldGoalThreshold = physicsParams["FourthDownFakeFGStrengthThreshold"].Value;
                        if (estimatedStrengthRatio >= fakeFieldGoalThreshold)
                        {
                            var selectionChance = physicsParams["FourthDownFakeFieldGoalSelectionChance"].Value;
                            if (parameters.Random.Chance(selectionChance))
                            {
                                Log.Verbose("MainGameDecision: Attempting a fake field goal on fourth down!");
                                return priorState.WithNextState(GameplayNextState.FakeFieldGoalOutcome);
                            }
                        }
                        Log.Verbose("MainGameDecision: Attempting a field goal on fourth down.");
                        return FieldGoalAttemptOutcome.Run(priorState, parameters, physicsParams);
                    }
                    else if (!isFakePlay)
                    {
                        var fakePuntThreshold = physicsParams["FourthDownFakePuntStrengthThreshold"].Value;
                        if (estimatedStrengthRatio >= fakePuntThreshold)
                        {
                            var selectionChance = physicsParams["FourthDownFakePuntSelectionChance"].Value;
                            if (parameters.Random.Chance(selectionChance))
                            {
                                Log.Verbose("MainGameDecision: Attempting a fake punt on fourth down!");
                                return priorState.WithNextState(GameplayNextState.FakePuntOutcome);
                            }
                        }
                        Log.Verbose("MainGameDecision: Punting on fourth down.");
                        return priorState.WithNextState(GameplayNextState.PuntOutcome);
                    }
                    return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                }
                else if (priorState.PeriodNumber > 4)
                {
                    if (parameters.GameType == GameType.Postseason)
                    {
                        Log.Verbose("MainGameDecision: Overtime in postseason, running play.");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }

                    var secondsLeft = priorState.TotalSecondsLeftInGame();
                    var fortySecondChunks = Math.Ceiling(secondsLeft / 40.0);
                    var downsLeft = priorState.NextPlay switch
                    {
                        NextPlayKind.FirstDown => 3,
                        NextPlayKind.SecondDown => 2,
                        NextPlayKind.ThirdDown => 1,
                        _ => 0
                    };

                    if (fortySecondChunks >= downsLeft)
                    {
                        Log.Verbose("MainGameDecision: Overtime with clock to burn, running play.");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }
                    else if (priorState.GetScoreDifferenceForTeam(self) > 0)
                    {
                        var lineOfScrimmageTeamYard = priorState.InternalYardToTeamYard(priorState.LineOfScrimmage);
                        if (lineOfScrimmageTeamYard.Team == self && lineOfScrimmageTeamYard.TeamYard >= fortySecondChunks)
                        {
                            Log.Verbose("MainGameDecision: Want to take victory formation but too close to own endzone, running play.");
                            return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                        }
                        else
                        {
                            Log.Verbose("MainGameDecision: Overtime leading and there's not enough time for opponent to win. Victory formation!");
                            return PlayerDownedFunction.Get(priorState, parameters, physicsParams, priorState.LineOfScrimmage, -1,
                                EndzoneBehavior.StandardGameplay, null);
                        }
                    }
                }
            }
        }

        private static GameState RunCore(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            double adjustedSelfPassingStrength)
        {
            var self = priorState.TeamWithPossession;
            var opponent = self.Opponent();
            var selfStrengths = parameters.GetActualStrengthsForTeam(self);
            var opponentStrengths = parameters.GetActualStrengthsForTeam(opponent);
            var selfEstimateOfSelf = parameters.GetEstimateOfTeamByTeam(self, self);
            var selfEstimateOfOpponent = parameters.GetEstimateOfTeamByTeam(self, opponent);
            var lineOfScrimmageTeamYard = priorState.InternalYardToTeamYard(priorState.LineOfScrimmage);
            var isFakePlay = priorState.GetAdditionalParameterOrDefault<bool?>("IsFakePlay") == true;

            var r = Math.Abs(selfEstimateOfSelf.RunningOffenseStrength - selfEstimateOfOpponent.RunningDefenseStrength);
            var p = Math.Abs(adjustedSelfPassingStrength - selfEstimateOfOpponent.PassingDefenseStrength);
            var passDesirability = p / (p + r);
            var passPlaySelected = parameters.Random.Chance(passDesirability);

            if (passPlaySelected)
            {
                if (priorState.TotalSecondsLeftInGame() < 10 &&
                    (priorState.PeriodNumber == 4 || parameters.GameType != GameType.Postseason))
                {
                    var fieldGoalRangeYard = physicsParams["FieldGoalRangeYard"].Value.Round();
                    bool inFieldGoalRange = lineOfScrimmageTeamYard.Team == opponent
                        && lineOfScrimmageTeamYard.TeamYard <= fieldGoalRangeYard;
                    if (!isFakePlay && inFieldGoalRange && CanAttemptFieldGoal(priorState))
                    {
                        Log.Verbose("MainGameDecision: Late-game passing play in field goal range, attempting field goal.");
                        return FieldGoalAttemptOutcome.Run(priorState, parameters, physicsParams);
                    }
                    Log.Verbose("MainGameDecision: Late-game passing play but not in field goal range, attempting Hail Mary.");
                    return priorState.WithNextState(GameplayNextState.HailMaryOutcome);
                }

                var selfDisposition = parameters.GetDispositionForTeam(self);
                if (selfDisposition == TeamDisposition.UltraConservative)
                {
                    Log.Verbose("MainGameDecision: Ultra-conservative team disposition, opting for a short pass.");
                    return priorState.WithNextState(GameplayNextState.StandardShortPassingPlayOutcome);
                }
                else if (selfDisposition == TeamDisposition.Insane)
                {
                    Log.Verbose("MainGameDecision: Insane team disposition, opting for a long pass.");
                    return priorState.WithNextState(GameplayNextState.StandardLongPassingPlayOutcome);
                }

                var shortPassChance = physicsParams["ConservativePassShortChance"].Value;
                var mediumPassChance = physicsParams["ConservativePassMediumChance"].Value;
                var longPassChance = physicsParams["ConservativePassLongChance"].Value;
                var nextDouble = parameters.Random.NextDouble();

                if (nextDouble < shortPassChance)
                {
                    Log.Verbose("MainGameDecision: Conservative team disposition, selected a medium pass.");
                    return priorState.WithNextState(GameplayNextState.StandardMediumPassingPlayOutcome);
                }
                else if (nextDouble < mediumPassChance)
                {
                    Log.Verbose("MainGameDecision: Conservative team disposition, selected a long pass.");
                    return priorState.WithNextState(GameplayNextState.StandardLongPassingPlayOutcome);
                }
            }

            Log.Verbose("MainGameDecision: Selected a rushing play.");
            return priorState.WithNextState(GameplayNextState.StandardRushingPlayOutcome);
        }

        /// <summary>
        /// Determines whether a field goal attempt is permitted in the current game state.
        /// Accounts for special play kinds (conversion attempts) and late-game overtime rules.
        /// </summary>
        /// <param name="gameState">The game state to evaluate for field goal eligibility.</param>
        /// <returns>
        /// True if a field goal may be attempted from the current state; otherwise false.
        /// </returns>
        private static bool CanAttemptFieldGoal(GameState gameState)
        {
            if (gameState.NextPlay == NextPlayKind.ConversionAttempt)
            {
                return false;
            }

            if (gameState.PeriodNumber > 4)
            {
                var scoreDifference = gameState.GetScoreDifferenceForTeam(gameState.TeamWithPossession);
                if (Math.Abs(scoreDifference) > 3)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
