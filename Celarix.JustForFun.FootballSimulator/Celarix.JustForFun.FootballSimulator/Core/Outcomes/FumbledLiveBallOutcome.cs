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
        public static GameState Run(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams)
        {
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
                    Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by offense and downed immediately at {Yard} yard line",
                        priorState.InternalYardToDisplayTeamYardString(fumbleRecoveryInternalYard.Round(), parameters));
                    return RunPlayerDownedFunction(priorState, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
                }
                
                if (fumbleRecoveryTeamYard.Team == fumbleRecoveryTeam.Opponent()
                    && fumbleRecoveryTeamYard.TeamYard <= 0)
                {
                    Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by offense inside defense's endzone, either a touchdown or successful two-point conversion!");
                    return RunPlayerDownedFunction(priorState, parameters, physicsParams, possessingTeamBeforeFumble,
                        fumbleRecoveryInternalYard);
                }

                Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by offense, eligible to return.");
                return ReturnFumbledOrInterceptedBallDecision.Run(priorState with
                {
                    LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled but {OffPlayer1} recovered at {LoS}."
                }, parameters, physicsParams);
            }
            else
            {
                if (fumbleDownedImmediately)
                {
                    Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by defense and downed immediately at {Yard} yard line",
                        priorState.InternalYardToDisplayTeamYardString(fumbleRecoveryInternalYard.Round(), parameters));
                    return RunPlayerDownedFunction(priorState with
                    {
                        TeamWithPossession = fumbleRecoveryTeam,
                        PossessionOnPlay = PossessionOnPlay.BothTeams,
                        LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                        LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble at {LoS}."
                    }, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
                }

                if (fumbleRecoveryTeamYard.Team == fumbleRecoveryTeam.Opponent()
                    && fumbleRecoveryTeamYard.TeamYard <= 0)
                {
                    Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by defense inside offense's endzone, either a touchdown or successful two-point conversion for the defense!");
                    return RunPlayerDownedFunction(priorState with
                    {
                        TeamWithPossession = fumbleRecoveryTeam,
                        PossessionOnPlay = PossessionOnPlay.BothTeams,
                        LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                        LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble inside {OffAbbr} endzone."
                    }, parameters, physicsParams, possessingTeamBeforeFumble, fumbleRecoveryInternalYard);
                }

                Log.Verbose("FumbledLiveBallOutcome: Fumble recovered by defense, eligible to return.");
                return ReturnFumbledOrInterceptedBallDecision.Run(priorState with
                {
                    TeamWithPossession = fumbleRecoveryTeam,
                    PossessionOnPlay = PossessionOnPlay.BothTeams,
                    LineOfScrimmage = fumbleRecoveryInternalYard.Round(),
                    LastPlayDescriptionTemplate = "{OffAbbr} {OffPlayer0} fumbled, {DefAbbr} {DefPlayer0} recovered fumble at {LoS}."
                }, parameters, physicsParams);
            }
        }

        private static GameState RunPlayerDownedFunction(GameState priorState,
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

            return PlayerDownedFunction.Get(priorState, parameters, physicsParams,
                        priorState.LineOfScrimmage,
                        fumbleRecoveryInternalYard.Round(),
                        endzoneBehavior,
                        (priorState.NextPlay == NextPlayKind.ConversionAttempt)
                            ? priorState.TeamWithPossession
                            : null);
        }
    }
}
