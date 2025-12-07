using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class NormalKickoffOutcome
    {
        public static GameState Run(GameState priorState,
           GameDecisionParameters parameters,
           IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
            GameTeam otherTeam = priorState.OtherTeam(priorState.TeamWithPossession);

            // Determine parameters for base kickoff distance
            double kickingStrength = parameters
                .GetActualStrengthsForTeam(priorState.TeamWithPossession)
                .KickingStrength;
            var baseKickDistanceMean = kickingStrength * 0.75;
            var baseKickDistanceStddev = physicsParams["KickoffDistanceStddev"].Value;
            var baseKickDistance = parameters.Random.SampleNormalDistribution(
                baseKickDistanceMean,
                baseKickDistanceStddev);

            // Determine parameters for base kickoff skew
            var alpha = Math.Log10(kickingStrength);
            var beta = 3 - alpha;
            var baseKickSkewMean = Math.Pow(10, beta);
            var baseKickSkewStddev = physicsParams["KickoffSkewStddev"].Value;
            var skewIsInverted = parameters.Random.Chance(0.5);
            var baseKickSkew = parameters.Random.SampleNormalDistribution(
                baseKickSkewMean,
                baseKickSkewStddev);
            if (skewIsInverted) { baseKickSkew *= -1; }

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
            var kickActualForwardDistance = baseKickDistance + windDirectComponent;
            var kickActualCross = baseKickSkew + windCrossComponent;
            var kickActualYard = priorState.AddYardsForPossessingTeam(priorState.LineOfScrimmage,
                kickActualForwardDistance);

            // Since kickoffs always happen at cross = 0, we can directly use kickActualCross
            var ballOnField = kickActualCross >= Constants.LeftSidelineCross
                && kickActualCross <= Constants.RightSidelineCross
                && kickActualYard >= Constants.HomeEndLineYard
                && kickActualYard <= Constants.AwayEndLineYard;
            if (ballOnField)
            {
                var kickDistanceFromKickoffSpot = priorState.DistanceForPossessingTeam(priorState.LineOfScrimmage,
                    kickActualYard);
                if (kickDistanceFromKickoffSpot > 20 || kickDistanceFromKickoffSpot < 10)
                {
                    // Either a normal kickoff that can be fielded, or a kick so short it can't even be onsided
                    // We treat distances over 20 yards as hypothetically possible for the kicking team
                    // to recover, but in practice never possible.
                    return ReturnableKickOutcome.Run(priorState,
                        parameters,
                        physicsParams,
                        kickActualYard);
                }
                else if (kickDistanceFromKickoffSpot >= 10 && kickDistanceFromKickoffSpot <= 20)
                {
                    // Not intentionally an onside kick attempt, but so short that it can be recovered by
                    // either team anyway
                    return FumbledLiveBallOutcome.Run(priorState,
                        parameters,
                        physicsParams,
                        kickActualYard);
                }

                var kickActualTeamYard = priorState.InternalYardToTeamYard(kickActualYard.Round());

                if (kickActualTeamYard.Team == otherTeam && kickActualTeamYard.TeamYard < -10d)
                {
                    // Touchback, ball kicked out of endzone
                    
                    return priorState with
                    {
                        TeamWithPossession = otherTeam,
                        LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                        LineToGain = priorState.TeamYardToInternalYard(otherTeam, 45),
                        NextPlay = NextPlayKind.FirstDown,
                    };
                }
                else if (kickActualTeamYard.Team == priorState.TeamWithPossession && kickActualTeamYard.TeamYard < -10d)
                {
                    // Safety, ball kicked out of own endzone
                    var updatedState = priorState.WithScoreChange(otherTeam, 2) with
                    {
                        LineOfScrimmage = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 20),
                    };
                    return FreeKickDecision.Run(updatedState,
                        parameters,
                        physicsParams);
                }
            }

            // Out of bounds kickoff
            return priorState with
            {
                TeamWithPossession = otherTeam,
                LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 45),
                LineToGain = priorState.TeamYardToInternalYard(priorState.TeamWithPossession, 45),
                NextPlay = NextPlayKind.FirstDown,
            };
        }
    }
}
