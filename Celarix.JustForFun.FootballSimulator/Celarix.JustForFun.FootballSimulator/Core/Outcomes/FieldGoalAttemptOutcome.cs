using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class FieldGoalAttemptOutcome
    {
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var desiredTargetYard = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), -10);
            var desiredCrossRangeMinimum = -3.08;
            var desiredCrossRangeMaximum = 3.08;
            var fieldGoalBaseDistanceMeanMultiplier = physicsParams["FieldGoalBaseDistanceMean"].Value;
            var fieldGoalBaseDistanceStdDev = physicsParams["FieldGoalBaseDistanceStdDev"].Value;
            var kickingTeamActualStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var kickingStrength = kickingTeamActualStrengths.KickingStrength;

            // Field goal base distance
            var fieldGoalBaseDistanceMean = kickingStrength * fieldGoalBaseDistanceMeanMultiplier;
            var fieldGoalBaseDistance = parameters.Random.SampleNormalDistribution(fieldGoalBaseDistanceMean, fieldGoalBaseDistanceStdDev);

            // Field goal base skew
            var alpha = Math.Log10(kickingStrength);
            var beta = 3 - alpha;
            var fieldGoalSkewMean = Math.Pow(10, beta);
            var fieldGoalSkewStdDev = physicsParams["FieldGoalSkewStdDev"].Value;
            var fieldGoalSkew = parameters.Random.SampleNormalDistribution(fieldGoalSkewMean, fieldGoalSkewStdDev);
            var fieldGoalKickingSkewMultiplier = physicsParams["FieldGoalKickingSkewMultiplier"].Value;
            fieldGoalSkew *= fieldGoalKickingSkewMultiplier;
            var skewInverted = parameters.Random.Chance(0.5);
            if (skewInverted)
            {
                fieldGoalSkew = -fieldGoalSkew;
            }

            var desiredKickDirection = desiredTargetYard == -10
                ? 0.0
                : 180.0;
            var windVarianceMean = priorState.BaseWindDirection;
            var windVarianceStdDev = physicsParams["WindAngleVarianceStddev"].Value;
            var currentWindAngle = parameters.Random.SampleNormalDistribution(windVarianceMean, windVarianceStdDev);
            var windSpeedVarianceMean = priorState.BaseWindSpeed;
            var windSpeedVarianceStdDev = physicsParams["WindSpeedVarianceStddev"].Value;
            var currentWindSpeed = Math.Max(0.0, parameters.Random.SampleNormalDistribution(windSpeedVarianceMean, windSpeedVarianceStdDev));
            var actualKickLocation = Math.Clamp(0.1,
                priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage, -15),
                109.9);

            if (actualKickLocation < 0 || actualKickLocation > 100)
            {
                Log.Verbose("FieldGoalAttemptOutcome: Applying endzone skew penalty");
                var yardsDeepInOwnEndzone = actualKickLocation < 0
                    ? -actualKickLocation
                    : actualKickLocation - 100.0;
                var penaltyPerYard = physicsParams["FieldGoalFromEndzoneSkewMultiplierPerYard"].Value;
                var totalSkewPenalty = Math.Pow(penaltyPerYard, yardsDeepInOwnEndzone);
                fieldGoalSkew += skewInverted
                    ? totalSkewPenalty
                    : -totalSkewPenalty;
            }

            // Compute wind effect on kick
            var adjustedWindAngle = desiredKickDirection == 0
                ? currentWindAngle
                : (currentWindAngle + 180) % 360;
            var windCrossComponent = currentWindSpeed * Math.Sin(adjustedWindAngle * Math.PI / 180);
            var windDirectComponent = currentWindSpeed * Math.Cos(adjustedWindAngle * Math.PI / 180);
            var ballWindYardsPerMPH = physicsParams["BallWindYardsPerMPH"].Value;
            windCrossComponent *= ballWindYardsPerMPH;
            windDirectComponent *= ballWindYardsPerMPH;

            // Find actual kick landing position
            var puntActualForwardDistance = fieldGoalBaseDistance + windDirectComponent;
            var kickActualCross = fieldGoalSkew + windCrossComponent;
            var kickActualYard = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage,
                puntActualForwardDistance);

            // Check if the kick made it
            var kickClearsDistance = priorState.CompareYardForTeam(kickActualYard, desiredTargetYard, priorState.TeamWithPossession) >= 0;
            var kickWithinSkew = kickActualCross >= desiredCrossRangeMinimum && kickActualCross <= desiredCrossRangeMaximum;
            var kickClearsUprights = KickClearsUprights(actualKickLocation, kickActualYard, desiredTargetYard);
            var kickSuccessful = kickClearsDistance && kickWithinSkew && kickClearsUprights;
            var kickBlocked = parameters.Random.Chance(GetKickBlockedChance(Math.Abs(actualKickLocation - desiredTargetYard)));

            if (kickBlocked)
            {
                Log.Verbose("FieldGoalAttemptOutcome: Kick was blocked, ball is live!");
                return priorState.WithNextState(GameplayNextState.FumbledLiveBallOutcome);
            }

            if (priorState.NextPlay != NextPlayKind.ConversionAttempt)
            {
                if (kickSuccessful)
                {
                    Log.Verbose("FieldGoalAttemptOutcome: Field goal kick was good!");
                    return priorState.WithScoreChange(priorState.TeamWithPossession, 3)
                        .WithNextState(GameplayNextState.PlayEvaluationComplete)
                    with
                    {
                        NextPlay = NextPlayKind.Kickoff,
                        LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35),
                        LineToGain = null,
                        ClockRunning = false,
                        LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} made a field goal from {FGKickDistance} yards."
                    };
                }
                else
                {
                    Log.Verbose("FieldGoalAttemptOutcome: Field goal kick was no good.");
                    return priorState.WithFirstDownLineOfScrimmage(priorState.LineOfScrimmage, priorState.TeamWithPossession.Opponent(),
                        "{DefAbbr} {DefPlayer0} missed a field goal from {FGKickDistance} yards, first down for {OffAbbr}.");
                }
            }

            if (kickSuccessful)
            {
                Log.Verbose("FieldGoalAttemptOutcome: Extra point kick was good!");
                return priorState.WithScoreChange(priorState.TeamWithPossession, 1)
                    .WithNextState(GameplayNextState.PlayEvaluationComplete)
                with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} made the extra point."
                };

            }
            else
            {
                Log.Verbose("FieldGoalAttemptOutcome: Extra point kick was no good.");
                return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 35),
                    LineToGain = null,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} missed the extra point."
                };
            }
        }

        private static bool KickClearsUprights(double kickOriginYard, double adjustedLandingYard, double uprightsYard, double v = 40.0, double arcFactor = 1.0)
        {
            // constants
            const double g = 10.724682868445;          // yards / s^2
            const double crossbarYards = 10.0 / 3.0;  // 10 ft -> yards

            // horizontal distances (from origin)
            double R = adjustedLandingYard - kickOriginYard;          // range where ball would land (yards)
            double x = uprightsYard - kickOriginYard;         // horizontal distance to uprights (yards)

            if (R <= 0 || x <= 0) return false; // nonsense / behind the kicker

            // adjust effective speed to allow for "higher" or "lower" arcs.
            // larger arcFactor (>1) => reduced effective speed => higher angle
            double v_eff = v / Math.Sqrt(Math.Max(1e-6, arcFactor));

            // compute sin(2θ) from projectile range formula: R = v^2 * sin(2θ) / g
            double sin2theta = (g * R) / (v_eff * v_eff);

            if (sin2theta >= 1.0)
            {
                // mathematically would require 2θ = 90° => θ = 45°, extreme high arc.
                // treat as the highest feasible angle (θ = 45°)
                double theta = Math.PI / 4.0;
                double heightYards = x * Math.Tan(theta) - (g * x * x) / (2.0 * v_eff * v_eff * Math.Cos(theta) * Math.Cos(theta));
                return heightYards >= crossbarYards;
            }
            if (sin2theta <= -1.0)
            {
                // impossible physically; treat as failure
                return false;
            }

            double thetaHalf = 0.5 * Math.Asin(sin2theta); // theta in radians

            // compute height at distance x (parabolic projectile)
            double heightYardsCalc = x * Math.Tan(thetaHalf) - (g * x * x) / (2.0 * v_eff * v_eff * Math.Cos(thetaHalf) * Math.Cos(thetaHalf));

            return heightYardsCalc >= crossbarYards;
        }

        private static double GetKickBlockedChance(double distanceFromTarget)
        {
            if (distanceFromTarget < 60)
            {
                return 0.01;
            }

            distanceFromTarget -= 60;
            return 0.01 + (distanceFromTarget * 0.005);
        }
    }
}
