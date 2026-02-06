using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class NormalKickoffOutcome
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            GameTeam otherTeam = priorState.TeamWithPossession.Opponent();

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

            var desiredKickDirection = object.GetFacingEndzoneAngle(priorState.TeamWithPossession);

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
                    Log.Information("NormalKickoffOutcome: Normal kick either abnormally short or too far for the kicking team to recover.");
                    priorState.AddTag("unusually-short-kickoff");
                    return priorState.WithNextState(PlayEvaluationState.ReturnableKickOutcome)
                        .WithAdditionalParameter("KickActualYard", kickActualYard.Round())
                        .InvolvesKick()
                        .InvolvesAdditionalOffensivePlayer() with
                    {
                        ClockRunning = true
                    };
                }
                else if (kickDistanceFromKickoffSpot >= 10 && kickDistanceFromKickoffSpot <= 20)
                {
                    Log.Information("NormalKickoffOutcome: Unintentionally short normal kick, can be recovered by either team.");
                    return priorState.WithNextState(PlayEvaluationState.FumbledLiveBallOutcome)
                        .InvolvesKick()
                        .InvolvesAdditionalOffensivePlayer() with
                    {
                        LineOfScrimmage = kickActualYard.Round(),
                        ClockRunning = true
                    };
                }

                var kickActualTeamYard = object.InternalYardToTeamYard(kickActualYard.Round());
                var kickActualTeamDisplayYard = priorState.InternalYardToDisplayTeamYardString(kickActualYard.Round(),
                    parameters);

                if (kickActualTeamYard.Team == otherTeam && kickActualTeamYard.TeamYard < -10d)
                {
                    Log.Information("NormalKickoffOutcome: Touchback on kickoff.");
                    priorState.AddTag("touchback");
                    return priorState.WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                        .InvolvesKick()
                        .InvolvesAdditionalOffensivePlayer() with
                    {
                        TeamWithPossession = otherTeam,
                        PossessionOnPlay = otherTeam.ToPossessionOnPlay(),
                        LineOfScrimmage = object.TeamYardToInternalYard(otherTeam, 35),
                        LineToGain = object.TeamYardToInternalYard(otherTeam, 45),
                        NextPlay = NextPlayKind.FirstDown,
                        ClockRunning = false,
                        LastPlayDescriptionTemplate = "{DefAbbr} touchback, ball placed at {LoS}."
                    };
                }
                else if (kickActualTeamYard.Team == priorState.TeamWithPossession && kickActualTeamYard.TeamYard < -10d)
                {
                    Log.Information("NormalKickoffOutcome: Kicking team safety on kickoff.");
                    priorState.AddTag("safety-scored");
                    priorState.AddTag("kickoff-backward-safety");
                    var updatedState = priorState.WithScoreChange(otherTeam, 2)
                        .InvolvesKick()
                        .InvolvesAdditionalOffensivePlayer() with
                    {
                        PossessionOnPlay = priorState.TeamWithPossession.ToPossessionOnPlay(),
                        NextPlay = NextPlayKind.FreeKick,
                        LineOfScrimmage = object.TeamYardToInternalYard(priorState.TeamWithPossession, 20),
                        ClockRunning = false,
                        LastPlayDescriptionTemplate = "{OffAbbr} kickoff results in kicking team safety. {DefAbbr} awarded 2 points. Free kick to follow from {LoS}.",
                        DriveResult = DriveResult.Safety
                    };
                    return updatedState.WithNextState(PlayEvaluationState.FreeKickDecision);
                }
            }

            Log.Information("NormalKickoffOutcome: Out of bounds kickoff.");
            priorState.AddTag("kickoff-out-of-bounds");
            return priorState.WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                .InvolvesKick()
                .InvolvesAdditionalOffensivePlayer() with
            {
                TeamWithPossession = otherTeam,
                PossessionOnPlay = otherTeam.ToPossessionOnPlay(),
                LineOfScrimmage = object.TeamYardToInternalYard(otherTeam, 40),
                LineToGain = object.TeamYardToInternalYard(priorState.TeamWithPossession, 50),
                NextPlay = NextPlayKind.FirstDown,
                ClockRunning = false,
                LastPlayDescriptionTemplate = "{OffAbbr} kickoff out of bounds, ball placed at {LoS}, first down."
            };
        }
    }
}
