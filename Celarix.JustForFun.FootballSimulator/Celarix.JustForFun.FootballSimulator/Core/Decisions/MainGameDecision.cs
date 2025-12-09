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
                return HailMaryOutcome.Run(priorState, parameters, physicsParams);
            }

            var clockDisposition = GetClockDisposition(priorState, parameters, physicsParams);
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
                            return QuarterbackSneakOutcome.Run(priorState, parameters, physicsParams);
                        }
                        Log.Verbose("MainGameDecision: Going for it on fourth down with a standard play!");
                        return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                    }

                    if (CanAttemptFieldGoal(priorState) && inFieldGoalRange)
                    {
                        var fakeFieldGoalThreshold = physicsParams["FourthDownFakeFGStrengthThreshold"].Value;
                        if (estimatedStrengthRatio >= fakeFieldGoalThreshold)
                        {
                            var selectionChance = physicsParams["FourthDownFakeFieldGoalSelectionChance"].Value;
                            if (parameters.Random.Chance(selectionChance))
                            {
                                Log.Verbose("MainGameDecision: Attempting a fake field goal on fourth down!");
                                return FakeFieldGoalOutcome.Run(priorState, parameters, physicsParams);
                            }
                        }
                        Log.Verbose("MainGameDecision: Attempting a field goal on fourth down.");
                        return FieldGoalAttemptOutcome.Run(priorState, parameters, physicsParams);
                    }
                    else
                    {
                        var fakePuntThreshold = physicsParams["FourthDownFakePuntStrengthThreshold"].Value;
                        if (estimatedStrengthRatio >= fakePuntThreshold)
                        {
                            var selectionChance = physicsParams["FourthDownFakePuntSelectionChance"].Value;
                            if (parameters.Random.Chance(selectionChance))
                            {
                                Log.Verbose("MainGameDecision: Attempting a fake punt on fourth down!");
                                return FakePuntOutcome.Run(priorState, parameters, physicsParams);
                            }
                        }
                        Log.Verbose("MainGameDecision: Punting on fourth down.");
                        return PuntOutcome.Run(priorState, parameters, physicsParams);
                    }
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
                    if (inFieldGoalRange && CanAttemptFieldGoal(priorState))
                    {
                        Log.Verbose("MainGameDecision: Late-game passing play in field goal range, attempting field goal.");
                        return FieldGoalAttemptOutcome.Run(priorState, parameters, physicsParams);
                    }
                    Log.Verbose("MainGameDecision: Late-game passing play but not in field goal range, attempting Hail Mary.");
                    return HailMaryOutcome.Run(priorState, parameters, physicsParams);
                }

                var selfDisposition = parameters.GetDispositionForTeam(self);
                if (selfDisposition == TeamDisposition.UltraConservative)
                {
                    Log.Verbose("MainGameDecision: Ultra-conservative team disposition, opting for a short pass.");
                    return ShortPassOutcome.Run(priorState, parameters, physicsParams);
                }
                else if (selfDisposition == TeamDisposition.Insane)
                {
                    Log.Verbose("MainGameDecision: Insane team disposition, opting for a long pass.");
                    return LongPassOutcome.Run(priorState, parameters, physicsParams);
                }

                var shortPassChance = physicsParams["ConservativePassShortChance"].Value;
                var mediumPassChance = physicsParams["ConservativePassMediumChance"].Value;
                var longPassChance = physicsParams["ConservativePassLongChance"].Value;
                var nextDouble = parameters.Random.NextDouble();

                if (nextDouble < shortPassChance)
                {
                    Log.Verbose("MainGameDecision: Conservative team disposition, selected a medium pass.");
                    return MediumPassOutcome.Run(priorState, parameters, physicsParams);
                }
                else if (nextDouble < mediumPassChance)
                {
                    Log.Verbose("MainGameDecision: Conservative team disposition, selected a long pass.");
                    return LongPassOutcome.Run(priorState, parameters, physicsParams);
                }
            }

            Log.Verbose("MainGameDecision: Selected a rushing play.");
            return RunningPlayOutcome.Run(priorState, parameters, physicsParams);
        }

        /// <summary>
        /// Determines the appropriate clock management disposition (for example, hurry-up
        /// or clock-chewing) for the team in possession based on the current game state,
        /// team dispositions and tuning parameters.
        /// </summary>
        /// <param name="priorState">The game state used to evaluate score, possession and time.</param>
        /// <param name="parameters">Decision parameters that include team dispositions and team strength estimates.</param>
        /// <param name="physicsParams">
        /// Dictionary of physics/tuning parameters. Expected keys used by this method:
        /// "LeadingClockDispositionInStandardZoneOpponentStrengthMultiple",
        /// "LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple",
        /// "LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay".
        /// </param>
        /// <returns>The chosen <see cref="ClockDisposition"/> for the possessing team.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when an unexpected <see cref="ClockZone"/> value is encountered.
        /// </exception>
        private static ClockDisposition GetClockDisposition(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            GameTeam self = priorState.TeamWithPossession;
            GameTeam opponent = self.Opponent();
            var clockZone = GetClockZone(priorState, physicsParams);

            var possessingTeamDisposition = parameters.GetDispositionForTeam(self);
            if (possessingTeamDisposition is TeamDisposition.UltraInsane)
            {
                return ClockDisposition.TwoMinuteDrill;
            }

            ClockDisposition selectedClockDisposition;
            if (priorState.GetScoreForTeam(self) > priorState.GetScoreForTeam(opponent))
            {
                var selfEstimateOfSelf = parameters.GetEstimateOfTeamByTeam(self, self);
                var selfEstimateOfOpponent = parameters.GetEstimateOfTeamByTeam(self, opponent);
                var averageRatio = selfEstimateOfSelf.OverallAverageStrength / selfEstimateOfOpponent.OverallAverageStrength;
                var highThreshold = physicsParams["LeadingClockDispositionInStandardZoneOpponentStrengthMultiple"].Value;
                var mediumThreshold = physicsParams["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultiple"].Value;
                var lowThreshold = physicsParams["LeadingClockDispositionInEndOfHalfZoneOpponentStrengthMultipleForAggressivePlay"].Value;

                if (clockZone == ClockZone.Standard)
                {
                    if (averageRatio > highThreshold)
                    {
                        selectedClockDisposition = ClockDisposition.ClockChewing;
                    }
                    else
                    {
                        selectedClockDisposition = ClockDisposition.Relaxed;
                    }
                }
                else if (averageRatio > mediumThreshold)
                {
                    if (averageRatio > lowThreshold)
                    {
                        selectedClockDisposition = ClockDisposition.TwoMinuteDrill;
                    }
                    else
                    {
                        selectedClockDisposition = ClockDisposition.HurryUp;
                    }
                }
                else
                {
                    selectedClockDisposition = ClockDisposition.Relaxed;
                }
            }
            else
            {
                selectedClockDisposition = clockZone switch
                {
                    ClockZone.Standard => ClockDisposition.HurryUp,
                    ClockZone.EndOfHalf => ClockDisposition.TwoMinuteDrill,
                    _ => throw new InvalidOperationException($"Unexpected clock zone {clockZone}.")
                };
            }

            if (possessingTeamDisposition == TeamDisposition.Insane &&
                    selectedClockDisposition is ClockDisposition.Relaxed or ClockDisposition.ClockChewing)
            {
                return ClockDisposition.HurryUp;
            }
            return selectedClockDisposition;
        }

        /// <summary>
        /// Classifies the current game time into a <see cref="ClockZone"/>. Uses the current
        /// period and the configured low-time threshold to determine if the game is in the
        /// standard time zone or the end-of-half time zone.
        /// </summary>
        /// <param name="priorState">The current game state used to obtain period and remaining time.</param>
        /// <param name="physicsParams">
        /// Dictionary of physics/tuning parameters. Expected key used by this method:
        /// "LowTimeInHalfThreshold".
        /// </param>
        /// <returns>The <see cref="ClockZone"/> representing whether the game is in standard play or end-of-half time.</returns>
        private static ClockZone GetClockZone(GameState priorState,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            if (priorState.PeriodNumber is 1 or 3)
            {
                return ClockZone.Standard;
            }

            var threshold = physicsParams["LowTimeInHalfThreshold"].Value.Round();
            var secondsLeftInGame = priorState.TotalSecondsLeftInGame();
            if (secondsLeftInGame <= threshold)
            {
                return ClockZone.EndOfHalf;
            }
            return ClockZone.Standard;
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
