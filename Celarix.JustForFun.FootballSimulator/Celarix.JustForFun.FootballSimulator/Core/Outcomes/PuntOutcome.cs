using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class PuntOutcome
    {
        public static PlayContext Run(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            var desiredTargetYard = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(), 0) switch
            {
                0 => 0.1d,
                100 => 99.9d
            };
            var teamStrengths = parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession);
            var kickingStrength = teamStrengths.KickingStrength;

            // Compute base kicking distance
            var meanMultiplier = physicsParams["PuntBaseDistanceMean"].Value;
            var stdDev = physicsParams["PuntBaseDistanceStddev"].Value;
            var baseKickDistance = parameters.Random.SampleNormalDistribution(kickingStrength * meanMultiplier, stdDev);

            // Compute base kicking skew
            var alpha = Math.Log10(kickingStrength);
            var beta = 3 - alpha;
            var kickSkewMean = Math.Pow(10, beta);
            var kickSkewStdDev = physicsParams["KickoffSkewStddev"].Value;
            var invertSkew = parameters.Random.Chance(0.5);
            var baseKickSkew = parameters.Random.SampleNormalDistribution(kickSkewMean, kickSkewStdDev);
            if (invertSkew) { baseKickSkew *= -1; }

            var desiredKickDirection = priorState.GetFacingEndzoneAngle(priorState.TeamWithPossession);

            // Get current wind conditions
            var currentWindAngle = parameters.Random.SampleNormalDistribution(
                priorState.BaseWindDirection,
                physicsParams["WindAngleVarianceStddev"].Value);
            var currentWindSpeed = parameters.Random.SampleNormalDistribution(
                priorState.BaseWindSpeed,
                physicsParams["WindSpeedVarianceStddev"].Value);
            var adjustedWindAngle = desiredKickDirection == 0
                ? currentWindAngle
                : (currentWindAngle + 180) % 360;
            var windCrossComponent = currentWindSpeed * Math.Sin(adjustedWindAngle * Math.PI / 180);
            var windDirectComponent = currentWindSpeed * Math.Cos(adjustedWindAngle * Math.PI / 180);
            var ballWindYardsPerMPH = physicsParams["BallWindYardsPerMPH"].Value;
            windCrossComponent *= ballWindYardsPerMPH;
            windDirectComponent *= ballWindYardsPerMPH;

            // Find actual kick landing position
            var puntActualForwardDistance = baseKickDistance + windDirectComponent;
            var kickActualCross = baseKickSkew + windCrossComponent;
            var kickActualYard = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage,
                puntActualForwardDistance);

            if (priorState.CompareYardForTeam(kickActualYard, desiredTargetYard, priorState.TeamWithPossession) < 0)
            {
                Log.Information("PuntOutcome: Punt fell short of target yard of 0.01. Skipping accuracy check.");
                return OutcomeDecision(kickActualYard,
                    priorState,
                    parameters,
                    physicsParams);
            }

            var ballOnField = kickActualCross >= Constants.LeftSidelineCross
                && kickActualCross <= Constants.RightSidelineCross
                && kickActualYard >= Constants.HomeEndLineYard
                && kickActualYard <= Constants.AwayEndLineYard;
            if (!ballOnField)
            {
                kickActualYard = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage,
                    ComputeKickOutOfBoundsForwardDistance(priorState.LineOfScrimmage, kickActualYard, kickActualCross));
            }

            // Compute kick accuracy
            var gamma = Math.Log2(kickingStrength);
            var delta = Math.Log2(100);
            var epsilon = gamma - delta;
            var kickAccuracyStddev = Math.Pow(2, epsilon);
            kickActualYard = priorState.TeamYardToInternalYard(priorState.TeamWithPossession.Opponent(),
                parameters.Random
                    .SampleNormalDistribution(desiredTargetYard, kickAccuracyStddev)
                    .Clamp(-10, 50)
                    .Round());
            Log.Information("PuntOutcome: Punt had distance to make target yard of 0.01, ran accuracy check.");
            return OutcomeDecision(kickActualYard,
                priorState,
                parameters,
                physicsParams);
        }

        private static PlayContext OutcomeDecision(double kickActualYard,
            PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            if (kickActualYard >= -10 && kickActualYard <= 110)
            {
                Log.Information("PuntOutcome: Punt landed in-bounds and can be recovered and returned.");
                return priorState.WithNextState(PlayEvaluationState.ReturnablePuntOutcome) with
                {
                    LineOfScrimmage = kickActualYard.Round(),
                    PossessionOnPlay = priorState.TeamWithPossession.ToPossessionOnPlay(),
                    ClockRunning = true,
                    LastPlayDescriptionTemplate = "{OffAbbr} {{OffPlayer0}} punt from {LoS} to {PuntYard} ({PuntDistance} yard(s))."
                };
            }

            var kickActualTeamYard = priorState.InternalYardToTeamYard(kickActualYard.Round());
            if (kickActualTeamYard.Team == priorState.TeamWithPossession)
            {
                Log.Information("PuntOutcome: Unusual punt resulted in a safety for the kicking team; went out back of own endzone.");
                var updatedState = priorState.WithScoreChange(kickActualTeamYard.Team.Opponent(), 2) with
                {
                    LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 20),
                    PossessionOnPlay = priorState.TeamWithPossession.ToPossessionOnPlay(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {{OffPlayer0}} punt from {LoS} out of back of own endzone for a safety."
                };
                return updatedState.WithNextState(PlayEvaluationState.FreeKickDecision);
            }

            Log.Information("PuntOutcome: Touchback.");
            return priorState.WithFirstDownLineOfScrimmage(25d, kickActualTeamYard.Team.Opponent(),
                "{DefAbbr} touchback, ball placed at {LoS}.", clockRunning: false, startOfDrive: true);
        }

        private static double ComputeKickOutOfBoundsForwardDistance(double lineOfScrimmage, double kickActualYard, double kickActualCross)
        {
            var kickedOverSidelineCross = kickActualCross < Constants.LeftSidelineCross
                ? Constants.LeftSidelineCross
                : Constants.RightSidelineCross;
            var crossDistanceBeyondSideline = kickActualCross - kickedOverSidelineCross;
            var portionOfKickOutOfBounds = Math.Abs(crossDistanceBeyondSideline) / Math.Abs(kickActualCross);
            var portionOfKickInBounds = 1 - portionOfKickOutOfBounds;
            var kickForwardDistance = kickActualYard - lineOfScrimmage;
            return kickForwardDistance * portionOfKickInBounds;
        }
    }
}
