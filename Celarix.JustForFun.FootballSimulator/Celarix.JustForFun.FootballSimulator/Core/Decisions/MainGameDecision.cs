using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Core.Outcomes;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Decisions
{
    internal static class MainGameDecision
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            GameTeam self = priorState.TeamWithPossession;
            GameTeam opponent = self.Opponent();

            // This... this is where the Ultra-Insane teams REALLY shine.
            var selfDisposition = parameters.GetDispositionForTeam(self);
            if (selfDisposition == TeamDisposition.UltraInsane)
            {
                Log.Information("MainGameDecision: Ultra-insane team disposition, ALWAYS Hail Mary!");
                for (var i = 0; i < 10; i++)
                {
                    Log.Verbose("Hail, Mary, full of grace, the Lord is with thee. Blessed art thou amongst women and blessed is the fruit of thy womb, Jesus. Holy Mary, Mother of God, pray for us sinners, now and at the hour of our death. Amen.");
                }
                priorState.AddTag("hail-mary-attempt");
                return priorState.WithNextState(PlayEvaluationState.HailMaryOutcome);
            }

            var isFakePlay = priorState.GetAdditionalParameterOrDefault<bool?>("IsFakePlay") == true;
            if (isFakePlay) { priorState.AddTag("fake-play"); }

            var clockDisposition = ClockDispositionFunction.Get(priorState);
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
                    return ClockRunningOnTwoMinuteDrill(priorState, parameters, physicsParams, adjustedPassingEstimate, self);
                }
                else
                {
                    var desirabilityMultiplier = physicsParams["TwoMinuteDrillPassDesirabilityMultiplier"].Value;
                    adjustedPassingEstimate *= desirabilityMultiplier;
                }
            }
            else if (priorState.NextPlay == NextPlayKind.FourthDown)
            {
                return FourthDown(priorState, parameters, physicsParams, adjustedPassingEstimate, isFakePlay, estimatedStrengthRatio);
            }
            else if (priorState.PeriodNumber > 4)
            {
                var overtimeResult = Overtime(priorState, parameters, physicsParams, self, adjustedPassingEstimate);
                if (overtimeResult != null)
                {
                    return overtimeResult;
                }
            }

            // Future self! If you're getting odd results here, that's because this call to RunCore
            // is a total guess because I didn't seem to finish this part of the tree in the design
            // document.
            return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
        }

        #region Outer Logic
        internal static PlayContext ClockRunningOnTwoMinuteDrill(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            double adjustedPassingEstimate,
            GameTeam self)
        {
            if (priorState.NextPlay == NextPlayKind.FourthDown)
            {
                Log.Information("MainGameDecision: Clock running during two-minute drill on fourth down, running play to avoid turnover on downs.");
                return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
            }

            var lineOfScrimmageTeamYard = object.InternalYardToTeamYard(priorState.LineOfScrimmage);
            var spikeBallYardLoss = physicsParams["SpikeBallYardLoss"].Value;
            if (lineOfScrimmageTeamYard.Team == self && lineOfScrimmageTeamYard.TeamYard <= spikeBallYardLoss)
            {
                Log.Information("MainGameDecision: Clock running during two-minute drill, but spiking the ball would result in a safety. Running play instead.");
                return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
            }

            var selfTimeoutsRemaining = priorState.TimeoutsRemainingForTeam(self);
            if (selfTimeoutsRemaining > 1)
            {
                Log.Information("MainGameDecision: Clock running during two-minute drill, calling a timeout.");
                priorState.AddTag("timeout");
                return priorState.TakeTimeout(self);
            }

            Log.Information("MainGameDecision: Spiking ball to stop the clock!");
            priorState.AddTag("spike-ball");
            return PlayerDownedFunction.Get(priorState, priorState.LineOfScrimmage, -2,
                priorState.NextPlay == NextPlayKind.ConversionAttempt ? EndzoneBehavior.ConversionAttempt : EndzoneBehavior.StandardGameplay,
                priorState.NextPlay == NextPlayKind.ConversionAttempt ? self : null);
        }

        internal static PlayContext FourthDown(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            double adjustedPassingEstimate,
            bool isFakePlay,
            double estimatedStrengthRatio)
        {
            var self = priorState.TeamWithPossession;
            var opponent = self.Opponent();
            bool goForIt = GoForItOnFourthDown(priorState, parameters, physicsParams, self, opponent);
            if (goForIt)
            {
                priorState.AddTag("fourth-down-attempt");
                return QBSneakOrRunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
            }

            if (CanAttemptFieldGoal(priorState) && !isFakePlay && InFieldGoalRange(priorState, physicsParams, opponent))
            {
                return FakeOrRealFieldGoal(priorState, parameters, physicsParams, estimatedStrengthRatio);
            }
            else if (!isFakePlay)
            {
                return FakeOrRealPunt(priorState, parameters, physicsParams, estimatedStrengthRatio);
            }

            // This code path only runs if it's a fake play already, so just run the play.
            return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
        }

        private static bool GoForItOnFourthDown(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, GameTeam self, GameTeam opponent)
        {
            double desireToGoForIt = 1.0;
            double desireToNotGoForIt = 1.0;

            desireToNotGoForIt *= GetFourthDownAntiDesireMultiplier_FieldGoalRange(priorState, physicsParams, opponent);
            desireToGoForIt *= GetFourthDownDesireMultiplier_TrailingCloseToEnd(priorState, physicsParams, self);
            desireToGoForIt *= GetFourthDownDesireMultiplier_DistanceToGo(priorState, physicsParams);
            desireToGoForIt *= GetFourthDownDesireMultiplier_FourthDownHistory(parameters, physicsParams, self);

            var ratio = desireToGoForIt / (desireToGoForIt + desireToNotGoForIt);
            var goForIt = parameters.Random.Chance(ratio);
            return goForIt;
        }
        
        internal static double GetFourthDownAntiDesireMultiplier_FieldGoalRange(PlayContext priorState,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            GameTeam opponent)
        {
            if (InFieldGoalRange(priorState, physicsParams, opponent))
            {
                var undesirabilityMultiplier = physicsParams["FourthDownLongFieldUndesirabilityMultiplier"].Value;
                return undesirabilityMultiplier;
            }
            return 1;
        }

        internal static double GetFourthDownDesireMultiplier_TrailingCloseToEnd(PlayContext priorState,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            GameTeam self)
        {
            var desirabilityThresholdAlpha = physicsParams["FourthDownTrailingNearEndgamePointThreshold"].Value.Round();
            var desirabilityThresholdBeta = physicsParams["FourthDownTrailingNearEndgameRemainingTimeThreshold"].Value;
            var desirabilityMultiplier = physicsParams["FourthDownTrailingNearEndgameDesirabilityMultiplier"].Value;
            var scoreDifference = priorState.GetScoreDifferenceForTeam(self);
            if (scoreDifference > desirabilityThresholdAlpha && priorState.TotalSecondsLeftInGame() < desirabilityThresholdBeta)
            {
                return desirabilityMultiplier;
            }
            return 1;
        }

        internal static double GetFourthDownDesireMultiplier_DistanceToGo(PlayContext priorState,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var desirabilityThresholdGamma = physicsParams["FourthDownDistanceToGoThreshold"].Value;
            var desirabilityMultiplier = physicsParams["FourthDownDistanceToGoDesirabilityMultiplier"].Value;
            var distanceToFirstDown = priorState.DistanceToEndzone();

            if (distanceToFirstDown < desirabilityThresholdGamma)
            {
                return desirabilityMultiplier;
            }
            return 1;
        }

        internal static double GetFourthDownDesireMultiplier_FourthDownHistory(GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            GameTeam self)
        {
            var desirabilityThresholdDelta = physicsParams["FourthDownHistoricalRateThreshold"].Value;
            var desirabilityMultiplier = physicsParams["FourthDownHistoricalRateDesirabilityMultiplier"].Value;
            if (parameters.GetFourthDownConversionRate(self) > desirabilityThresholdDelta)
            {
                return desirabilityMultiplier;
            }
            return 1;
        }

        private static PlayContext QBSneakOrRunCore(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, double adjustedPassingEstimate)
        {
            var qbSneakDistance = physicsParams["FourthDownQBSneakDistanceThreshold"].Value;
            if (priorState.DistanceToGo() <= qbSneakDistance)
            {
                Log.Information("MainGameDecision: Going for it on fourth down with a QB sneak!");
                priorState.AddTag("qb-sneak");
                return priorState.WithNextState(PlayEvaluationState.QBSneakOutcome);
            }
            Log.Information("MainGameDecision: Going for it on fourth down with a standard play!");
            return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
        }

        private static PlayContext FakeOrRealFieldGoal(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, double estimatedStrengthRatio)
        {
            var fakeFieldGoalThreshold = physicsParams["FourthDownFakeFGStrengthThreshold"].Value;
            if (estimatedStrengthRatio >= fakeFieldGoalThreshold)
            {
                var selectionChance = physicsParams["FourthDownFakeFieldGoalSelectionChance"].Value;
                if (parameters.Random.Chance(selectionChance))
                {
                    Log.Information("MainGameDecision: Attempting a fake field goal on fourth down!");
                    return priorState.WithNextState(PlayEvaluationState.FakeFieldGoalOutcome);
                }
            }
            Log.Information("MainGameDecision: Attempting a field goal on fourth down.");
            priorState.AddTag("field-goal-attempt");
            return FieldGoalAttemptOutcome.Run(priorState);
        }

        private static PlayContext FakeOrRealPunt(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, double estimatedStrengthRatio)
        {
            var fakePuntThreshold = physicsParams["FourthDownFakePuntStrengthThreshold"].Value;
            if (estimatedStrengthRatio >= fakePuntThreshold)
            {
                var selectionChance = physicsParams["FourthDownFakePuntSelectionChance"].Value;
                if (parameters.Random.Chance(selectionChance))
                {
                    Log.Information("MainGameDecision: Attempting a fake punt on fourth down!");
                    return priorState.WithNextState(PlayEvaluationState.FakePuntOutcome);
                }
            }
            Log.Information("MainGameDecision: Punting on fourth down.");
            priorState.AddTag("punt");
            return priorState.WithNextState(PlayEvaluationState.PuntOutcome);
        }

        internal static PlayContext? Overtime(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, GameTeam self, double adjustedPassingEstimate)
        {
            if (parameters.GameType == GameType.Postseason)
            {
                Log.Information("MainGameDecision: Overtime in postseason, running play.");
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
                Log.Information("MainGameDecision: Overtime with clock to burn, running play.");
                return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
            }
            else if (priorState.GetScoreDifferenceForTeam(self) > 0)
            {
                var lineOfScrimmageTeamYard = object.InternalYardToTeamYard(priorState.LineOfScrimmage);
                if (lineOfScrimmageTeamYard.Team == self && lineOfScrimmageTeamYard.TeamYard >= fortySecondChunks)
                {
                    Log.Information("MainGameDecision: Want to take victory formation but too close to own endzone, running play.");
                    return RunCore(priorState, parameters, physicsParams, adjustedPassingEstimate);
                }
                else
                {
                    Log.Information("MainGameDecision: Overtime leading and there's not enough time for opponent to win. Victory formation!");
                    priorState.AddTag("victory-formation");
                    return PlayerDownedFunction.Get(priorState, priorState.LineOfScrimmage, -1,
                        EndzoneBehavior.StandardGameplay, null);
                }
            }

            return null;
        }
        #endregion

        #region Inner Logic
        internal static PlayContext RunCore(PlayContext priorState,
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
            var lineOfScrimmageTeamYard = object.InternalYardToTeamYard(priorState.LineOfScrimmage);
            var isFakePlay = priorState.GetAdditionalParameterOrDefault<bool?>("IsFakePlay") == true;

            var r = Math.Abs(selfEstimateOfSelf.RunningOffenseStrength - selfEstimateOfOpponent.RunningDefenseStrength);
            var p = Math.Abs(adjustedSelfPassingStrength - selfEstimateOfOpponent.PassingDefenseStrength);
            var passDesirability = p / (p + r);
            var passPlaySelected = parameters.Random.Chance(passDesirability);

            if (passPlaySelected)
            {
                return PassingPlay(priorState, parameters, physicsParams, self, opponent, isFakePlay);
            }

            Log.Information("MainGameDecision: Selected a rushing play.");
            return priorState.WithNextState(PlayEvaluationState.StandardRushingPlayOutcome);
        }

        internal static PlayContext PassingPlay(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, GameTeam self, GameTeam opponent, bool isFakePlay)
        {
            if (priorState.TotalSecondsLeftInGame() < 10 &&
                                (priorState.PeriodNumber == 4 || parameters.GameType != GameType.Postseason))
            {
                return PassingVeryCloseToEndOfGame(priorState, parameters, physicsParams, opponent, isFakePlay);
            }

            var selfDisposition = parameters.GetDispositionForTeam(self);
            if (selfDisposition == TeamDisposition.UltraConservative)
            {
                Log.Information("MainGameDecision: Ultra-conservative team disposition, opting for a short pass.");
                return priorState.WithNextState(PlayEvaluationState.StandardShortPassingPlayOutcome);
            }
            else if (selfDisposition == TeamDisposition.Insane)
            {
                Log.Information("MainGameDecision: Insane team disposition, opting for a long pass.");
                return priorState.WithNextState(PlayEvaluationState.StandardLongPassingPlayOutcome);
            }

            var shortPassChance = physicsParams["ConservativePassShortChance"].Value;
            var mediumPassChance = physicsParams["ConservativePassMediumChance"].Value;
            var longPassChance = physicsParams["ConservativePassLongChance"].Value;
            var nextDouble = parameters.Random.NextDouble();

            if (nextDouble < shortPassChance)
            {
                Log.Information("MainGameDecision: Conservative team disposition, selected a short pass.");
                return priorState.WithNextState(PlayEvaluationState.StandardShortPassingPlayOutcome);
            }
            else if (nextDouble < mediumPassChance)
            {
                Log.Information("MainGameDecision: Conservative team disposition, selected a medium pass.");
                return priorState.WithNextState(PlayEvaluationState.StandardMediumPassingPlayOutcome);
            }

            Log.Information("MainGameDecision: Conservative team disposition, selected a long pass.");
            return priorState.WithNextState(PlayEvaluationState.StandardLongPassingPlayOutcome);
        }

        internal static PlayContext PassingVeryCloseToEndOfGame(PlayContext priorState, GameDecisionParameters parameters, IReadOnlyDictionary<string, PhysicsParam> physicsParams, GameTeam opponent, bool isFakePlay)
        {
            if (!isFakePlay && InFieldGoalRange(priorState, physicsParams, opponent) && CanAttemptFieldGoal(priorState))
            {
                Log.Information("MainGameDecision: Late-game passing play in field goal range, attempting field goal.");
                return FieldGoalAttemptOutcome.Run(priorState);
            }
            Log.Information("MainGameDecision: Late-game passing play but not in field goal range, attempting Hail Mary.");
            priorState.AddTag("hail-mary-attempt");
            return priorState.WithNextState(PlayEvaluationState.HailMaryOutcome);
        }
        #endregion

        #region Helpers
        internal static bool InFieldGoalRange(PlayContext priorState, IReadOnlyDictionary<string, PhysicsParam> physicsParams, GameTeam opponent)
        {
            bool inFieldGoalRange;
            var lineOfScrimmageTeamYard = object.InternalYardToTeamYard(priorState.LineOfScrimmage);
            var fieldGoalRangeYard = physicsParams["FieldGoalRangeYard"].Value.Round();
            inFieldGoalRange = lineOfScrimmageTeamYard.Team == opponent
                && lineOfScrimmageTeamYard.TeamYard <= fieldGoalRangeYard;
            return inFieldGoalRange;
        }


        /// <summary>
        /// Determines whether a field goal attempt is permitted in the current game state.
        /// Accounts for special play kinds (conversion attempts) and late-game overtime rules.
        /// </summary>
        /// <param name="gameState">The game state to evaluate for field goal eligibility.</param>
        /// <returns>
        /// True if a field goal may be attempted from the current state; otherwise false.
        /// </returns>
        internal static bool CanAttemptFieldGoal(PlayContext gameState)
        {
            if (gameState.NextPlay == NextPlayKind.ConversionAttempt)
            {
                // Yes, a field goal attempt on a conversion attempt is just an extra point attempt,
                // but if we got to this point in the logic tree, it's because we decided earlier
                // to go for two.
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
        #endregion
    }
}
