using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Core.Functions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Outcomes
{
    internal static class FumbledLiveBallOutcome
    {
        public static PlayContext Run(PlayContext priorState)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var wasInterception = priorState.GetAdditionalParameterOrDefault<bool?>("WasIntercepted");
            if (wasInterception == true)
            {
                // Interception! Check if we're in their endzone already.
                Log.Information("FumbledLiveBallOutcome: Interception occurred during passing play.");
                var interceptionInternalYard = priorState.LineOfScrimmage;
                var interceptionTeamYard = priorState.InternalYardToTeamYard(interceptionInternalYard);
                if (interceptionTeamYard.TeamYard is < 0 && interceptionTeamYard.Team == priorState.TeamWithPossession)
                {
                    Log.Information("FumbledLiveBallOutcome: Interception occurred inside offense's endzone, either a touchdown or successful two-point conversion for the defense!");
                    return RunPlayerDownedFunction(priorState.InvolvesAdditionalDefensivePlayer() with
                    {
                        TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                        PossessionOnPlay = PossessionOnPlay.BothTeams,
                        LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} pass intercepted by {OffPlayer0} inside {DefAbbr} endzone!"
                    }, parameters, physicsParams, priorState.TeamWithPossession, interceptionInternalYard);
                }
                else
                {
                    Log.Information("FumbledLiveBallOutcome: Interception occurred outside offense's endzone.");
                    return FumbleOrInteceptionRecoveredByDefense(priorState.InvolvesAdditionalDefensivePlayer() with
                    {
                        TeamWithPossession = priorState.TeamWithPossession.Opponent(),
                        PossessionOnPlay = PossessionOnPlay.BothTeams,
                        LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} pass intercepted.",
                        DriveResult = DriveResult.Interception
                    }, parameters, physicsParams, priorState.TeamWithPossession, priorState.TeamWithPossession.Opponent(),
                    fumbleDownedImmediately: false, priorState.LineOfScrimmage, interceptionTeamYard);
                }
            }

            var possessingTeamBeforeFumble = priorState.TeamWithPossession;
            var standardStrengthStddev = physicsParams["StandardStrengthStddev"].Value;
            var offensiveLineStrength = parameters.Random.SampleNormalDistribution(
                parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession).OffensiveLineStrength,
                standardStrengthStddev);
            var defensiveLineStrength = parameters.Random.SampleNormalDistribution(
                parameters.GetActualStrengthsForTeam(priorState.TeamWithPossession.Opponent()).DefensiveLineStrength,
                standardStrengthStddev);
            var sampleSum = offensiveLineStrength + defensiveLineStrength;
            var fumbleRecoveryChance = offensiveLineStrength / sampleSum;
            var fumbleRecoveredByOffense = parameters.Random.Chance(fumbleRecoveryChance);
            var fumbleRecoveryTeam = fumbleRecoveredByOffense
                ? priorState.TeamWithPossession
                : priorState.TeamWithPossession.Opponent();

            var fumbleRecoveryDistanceMean = physicsParams["FumbleRecoveryDistanceMean"].Value;
            var fumbleRecoveryDistanceStddev = physicsParams["FumbleRecoveryDistanceStddev"].Value;
            var fumbleRecoveryDistance = parameters.Random.SampleNormalDistribution(fumbleRecoveryDistanceMean, fumbleRecoveryDistanceStddev);
            var fumbledRolledBackward = parameters.Random.Chance(0.5);
            if (fumbledRolledBackward)
            {
                fumbleRecoveryDistance = -fumbleRecoveryDistance;
            }

            var downedImmediatelyChance = fumbleRecoveredByOffense
                ? physicsParams["FumbleRecoveredByOffenseChanceOfBeingDownedImmediately"].Value
                : physicsParams["FumbleRecoveredByOffenseChanceOfBeingDownedImmediately"].Value;
            var fumbleDownedImmediately = parameters.Random.Chance(downedImmediatelyChance);
            // It's okay to just use the + operator here, since the fumble has a 50% chance of going backwards,
            // it's fair for either direction to use + rather than the specialized function.
            var fumbleRecoveryInternalYard = priorState.LineOfScrimmage + fumbleRecoveryDistance;
            var fumbleRecoveryTeamYard = priorState.InternalYardToTeamYard(fumbleRecoveryInternalYard.Round());

            if (fumbleRecoveredByOffense)
            {
                if (fumbleDownedImmediately)
                {
                    Log.Information("FumbledLiveBallOutcome: Fumble recovered by offense and downed immediately at {Yard} yard line",
                        priorState.InternalYardToDisplayTeamYardString(fumbleRecoveryInternalYard.Round(), parameters));
                    return RunPlayerDownedFunction(priorState, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
                }
                
                if (fumbleRecoveryTeamYard.Team == fumbleRecoveryTeam.Opponent()
                    && fumbleRecoveryTeamYard.TeamYard <= 0)
                {
                    Log.Information("FumbledLiveBallOutcome: Fumble recovered by offense inside defense's endzone, either a touchdown or successful two-point conversion!");
                    return RunPlayerDownedFunction(priorState, parameters, physicsParams, possessingTeamBeforeFumble,
                        fumbleRecoveryInternalYard);
                }

                Log.Information("FumbledLiveBallOutcome: Fumble recovered by offense, eligible to return.");
                return priorState.WithNextState(PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision) with
                {
                    LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                    ClockRunning = true,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled but {OffPlayer1} recovered at {LoS}."
                };
            }
            else
            {
                return FumbleOrInteceptionRecoveredByDefense(priorState, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryTeam, fumbleDownedImmediately, fumbleRecoveryInternalYard, fumbleRecoveryTeamYard);
            }
        }

        private static PlayContext FumbleOrInteceptionRecoveredByDefense(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            GameTeam possessingTeamBeforeFumble,
            GameTeam fumbleRecoveryTeam,
            bool fumbleDownedImmediately,
            double fumbleRecoveryInternalYard,
            (GameTeam Team, int TeamYard) fumbleRecoveryTeamYard)
        {
            if (fumbleDownedImmediately)
            {
                Log.Information("FumbledLiveBallOutcome: Fumble recovered by defense and downed immediately at {Yard} yard line",
                    priorState.InternalYardToDisplayTeamYardString(fumbleRecoveryInternalYard.Round(), parameters));
                return RunPlayerDownedFunction(priorState with
                {
                    TeamWithPossession = fumbleRecoveryTeam,
                    PossessionOnPlay = PossessionOnPlay.BothTeams,
                    LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble at {LoS}.",
                    DriveResult = DriveResult.FumbleLost
                }, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
            }

            if (fumbleRecoveryTeamYard.Team == fumbleRecoveryTeam.Opponent()
                && fumbleRecoveryTeamYard.TeamYard <= 0)
            {
                Log.Information("FumbledLiveBallOutcome: Fumble recovered by defense inside offense's endzone, either a touchdown or successful two-point conversion for the defense!");
                return RunPlayerDownedFunction(priorState with
                {
                    TeamWithPossession = fumbleRecoveryTeam,
                    PossessionOnPlay = PossessionOnPlay.BothTeams,
                    LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble inside {OffAbbr} endzone.",
                    DriveResult = DriveResult.FumbleLost
                }, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
            }

            Log.Information("FumbledLiveBallOutcome: Fumble recovered by defense, eligible to return.");
            return priorState.WithNextState(PlayEvaluationState.ReturnFumbledOrInterceptedBallDecision) with
            {
                TeamWithPossession = fumbleRecoveryTeam,
                PossessionOnPlay = PossessionOnPlay.BothTeams,
                LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble at {LoS}.",
                DriveResult = DriveResult.FumbleLost
            };
        }

        private static PlayContext RunPlayerDownedFunction(PlayContext priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            GameTeam possessingTeamBeforeFumble,
            double fumbleRecoveryInternalYard)
        {
            EndzoneBehavior endzoneBehavior = (priorState.NextPlay == NextPlayKind.ConversionAttempt)
                            ? EndzoneBehavior.ConversionAttempt
                            : (priorState.TeamWithPossession != possessingTeamBeforeFumble)
                                ? EndzoneBehavior.FumbleOrInterceptionReturn
                                : EndzoneBehavior.StandardGameplay;

            return PlayerDownedFunction.Get(priorState,
                        priorState.LineOfScrimmage,
                        fumbleRecoveryInternalYard.Round(),
                        endzoneBehavior,
                        (priorState.NextPlay == NextPlayKind.ConversionAttempt)
                            ? priorState.TeamWithPossession
                            : null);
        }
    }
}
