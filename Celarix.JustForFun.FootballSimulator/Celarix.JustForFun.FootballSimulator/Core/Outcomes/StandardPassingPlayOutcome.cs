using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class StandardPassingPlayOutcome
    {
        public static PlayContext Run(PlayContext priorState,
           GameDecisionParameters parameters,
           IReadOnlyDictionary<string, PhysicsParam> physicsParams,
           PassAttemptDistance distance)
        {
            var selfStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var opponentStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent());
            var selfPassingStrength = selfStrengths.PassingOffenseStrength;
            var opponentPassingDefenseStrength = opponentStrengths.PassingDefenseStrength;
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;

            var selfSample = parameters.Random.SampleNormalDistribution(selfPassingStrength, standardStrengthStddev);
            var opponentSample = parameters.Random.SampleNormalDistribution(opponentPassingDefenseStrength, standardStrengthStddev);
            var ratio = selfSample / opponentSample;
            var inverseRatio = opponentSample / selfSample;

            var baseInterceptionChance = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassInterceptionChance",
                PassAttemptDistance.Medium => "BaseMediumPassInterceptionChance",
                PassAttemptDistance.Long => "BaseLongPassInterceptionChance",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var interceptionDoublingCost = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "ShortPassInterceptionDoublingCost",
                PassAttemptDistance.Medium => "MediumPassInterceptionDoublingCost",
                PassAttemptDistance.Long => "LongPassInterceptionDoublingCost",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;

            double interceptionChance = baseInterceptionChance;
            if (ratio < 1)
            {
                var doublings = inverseRatio / interceptionDoublingCost;
                interceptionChance = baseInterceptionChance * Math.Pow(2, doublings);
            }
            else
            {
                var halvings = ratio / interceptionDoublingCost;
                interceptionChance = baseInterceptionChance * Math.Pow(0.5, halvings);
            }
            interceptionChance = Math.Clamp(interceptionChance, 0, 0.99);
            var wasIntercepted = parameters.Random.Chance(interceptionChance);

            var shortPassDistanceThreshold = physicsParams["ShortPassDistanceThreshold"].Value;
            var mediumPassDistanceThreshold = physicsParams["MediumPassDistanceThreshold"].Value;
            var longPassDistanceThreshold = physicsParams["LongPassDistanceThreshold"].Value;
            var passRangeMinimum = distance switch
            {
                PassAttemptDistance.Short => -shortPassDistanceThreshold,
                PassAttemptDistance.Medium => shortPassDistanceThreshold + 1,
                PassAttemptDistance.Long => mediumPassDistanceThreshold + 1,
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            };
            var passRangeMaximum = distance switch
            {
                PassAttemptDistance.Short => shortPassDistanceThreshold,
                PassAttemptDistance.Medium => mediumPassDistanceThreshold,
                PassAttemptDistance.Long => longPassDistanceThreshold,
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            };
            var rangeSize = passRangeMaximum - passRangeMinimum;
            var sampledDouble = parameters.Random.NextDouble();
            var yardsGained = passRangeMinimum + (sampledDouble * rangeSize);
            var catchYard = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, yardsGained);
            catchYard = Math.Clamp(catchYard, Constants.HomeEndLineYard, Constants.AwayEndLineYard);

            if (wasIntercepted)
            {
                return priorState.WithAdditionalParameter<bool?>("WasIntercepted", true)
                    .WithNextState(PlayEvaluationState.FumbledLiveBallOutcome)
                with
                {
                    ClockRunning = true,
                    LineOfScrimmage = catchYard.Round()
                };
            }

            var baseCompletionChance = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassCompletionChance",
                PassAttemptDistance.Medium => "BaseMediumPassCompletionChance",
                PassAttemptDistance.Long => "BaseLongPassCompletionChance",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var completionChanceMultiplicationCost = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "ShortPassCompletionMultiplicationCost",
                PassAttemptDistance.Medium => "MediumPassCompletionMultiplicationCost",
                PassAttemptDistance.Long => "LongPassCompletionMultiplicationCost",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var completionMultiplierIfBad = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "ShortPassCompletionMultiplierIfBad",
                PassAttemptDistance.Medium => "MediumPassCompletionMultiplierIfBad",
                PassAttemptDistance.Long => "LongPassCompletionMultiplierIfBad",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var completionMultiplierIfGood = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "ShortPassCompletionMultiplierIfGood",
                PassAttemptDistance.Medium => "MediumPassCompletionMultiplierIfGood",
                PassAttemptDistance.Long => "LongPassCompletionMultiplierIfGood",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;

            if (ratio < 1)
            {
                var multiplications = inverseRatio / completionChanceMultiplicationCost;
                baseCompletionChance *= Math.Pow(completionMultiplierIfBad, multiplications);
            }
            else if (ratio > 1)
            {
                var multiplications = ratio / completionChanceMultiplicationCost;
                baseCompletionChance *= Math.Pow(completionMultiplierIfGood, multiplications);
            }
            baseCompletionChance = Math.Clamp(baseCompletionChance, 0, 1);
            var wasCompleted = parameters.Random.Chance(baseCompletionChance);
            
            if (!wasCompleted)
            {
                // Incomplete passes will be treated as 0-yard rushing gains.
                Log.Information("StandardPassingPlayOutcome: Incomplete pass.");
                return PlayerDownedFunction.Get(priorState, parameters, physicsParams,
                    priorState.LineOfScrimmage,
                    0,
                    EndzoneBehavior.StandardGameplay,
                    null,
                    clockRunning: false);
            }

            var baseYACChance = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassYACChance",
                PassAttemptDistance.Medium => "BaseMediumPassYACChance",
                PassAttemptDistance.Long => "BaseLongPassYACChance",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var yacMultiplicationCost = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassYACMultiplicationCost",
                PassAttemptDistance.Medium => "BaseMediumPassYACMultiplicationCost",
                PassAttemptDistance.Long => "BaseLongPassYACMultiplicationCost",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var yacMultiplierIfBad = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassYACMultiplierIfBad",
                PassAttemptDistance.Medium => "BaseMediumPassYACMultiplierIfBad",
                PassAttemptDistance.Long => "BaseLongPassYACMultiplierIfBad",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;
            var yacMultiplierIfGood = physicsParams[distance switch
            {
                PassAttemptDistance.Short => "BaseShortPassYACMultiplierIfGood",
                PassAttemptDistance.Medium => "BaseMediumPassYACMultiplierIfGood",
                PassAttemptDistance.Long => "BaseLongPassYACMultiplierIfGood",
                _ => throw new ArgumentOutOfRangeException(nameof(distance), distance, null)
            }].Value;

            if (ratio < 1)
            {
                var multiplications = inverseRatio / yacMultiplicationCost;
                baseYACChance *= Math.Pow(yacMultiplierIfBad, multiplications);
            }
            else if (ratio > 1)
            {
                var multiplications = ratio / yacMultiplicationCost;
                baseYACChance *= Math.Pow(yacMultiplierIfGood, multiplications);
            }
            baseYACChance = Math.Clamp(baseYACChance, 0, 1);
            var hadYAC = parameters.Random.Chance(baseYACChance);

            if (!hadYAC)
            {
                Log.Information("StandardPassingPlayOutcome: Ball caught for a gain of {YardsGained} but receiver brought down immediately.", yardsGained.Round());
                return PlayerDownedFunction.Get(priorState, parameters, physicsParams,
                    priorState.LineOfScrimmage,
                    yardsGained.Round(),
                    EndzoneBehavior.StandardGameplay,
                    null);
            }

            var yardsAfterCatch = UniversalRushingFunction.Get(priorState.LineOfScrimmage, selfStrengths.RunningOffenseStrength,
                opponentStrengths.RunningDefenseStrength,
                physicsParams,
                parameters.Random);

            if (yardsAfterCatch.WasFumbled)
            {
                Log.Information("StandardPassingPlayOutcome: Fumble after a catch for {YardsGained}.", yardsAfterCatch.YardsGained);
                return priorState.WithNextState(PlayEvaluationState.FumbledLiveBallOutcome);
            }

            int yardsPlusYAC = Math.Clamp(yardsGained + (yardsAfterCatch.YardsGained ?? 0),
                    Constants.HomeEndLineYard,
                    Constants.AwayEndLineYard).Round();
            Log.Information("StandardPassingPlayOutcome: Completed pass with YAC, total gain {YardsGained}.", yardsPlusYAC);
            return PlayerDownedFunction.Get(priorState, parameters, physicsParams,
                priorState.LineOfScrimmage,
                yardsPlusYAC,
                EndzoneBehavior.StandardGameplay,
                null);
        }
    }
}
