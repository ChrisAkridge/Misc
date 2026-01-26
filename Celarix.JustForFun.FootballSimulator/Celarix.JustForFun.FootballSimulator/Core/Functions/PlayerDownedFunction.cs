using Celarix.JustForFun.FootballSimulator.Core.Decisions;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Functions
{
    internal static class PlayerDownedFunction
    {
        public static PlayContext Get(PlayContext priorState,
            int possessionStartYard,
            int yardsGained,
            EndzoneBehavior endzoneBehavior,
            GameTeam? teamAttemptingConversion,
            bool? clockRunning = null)
        {
            var parameters = priorState.Environment!.DecisionParameters;
            var physicsParams = priorState.Environment.PhysicsParams;

            var possessingTeam = priorState.TeamWithPossession;
            var otherTeam = possessingTeam.Opponent();
            var newLineOfScrimmage = priorState.AddYardsForPossessingTeam(possessionStartYard, yardsGained).ClampWithinField();
            var gainedTeamYard = priorState.InternalYardToTeamYard(newLineOfScrimmage.Round());
            string displayYard = priorState.InternalYardToDisplayTeamYardString(newLineOfScrimmage.Round(), parameters);

            if (gainedTeamYard.TeamYard < 0)
            {
                if (gainedTeamYard.Team == possessingTeam)
                {
                    if (endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
                    {
                        Log.Information("PlayerDownedFunction: Fumble recovered in own endzone, touchback.");
                        return priorState.WithNextState(PlayEvaluationState.PlayEvaluationComplete) with
                        {
                            NextPlay = NextPlayKind.FirstDown,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 25),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = clockRunning ?? false,
                            LastPlayDescriptionTemplate =
                                "{OffAbbr} recovers fumble in own endzone, touchback. Ball placed at {LoS}."
                        };
                    }

                    // Safety! Oh, no!
                    var safetyPoints = endzoneBehavior switch
                    {
                        EndzoneBehavior.StandardGameplay => 2,
                        EndzoneBehavior.ConversionAttempt => 1,
                        _ => throw new ArgumentOutOfRangeException(nameof(endzoneBehavior), endzoneBehavior, null)
                    };

                    if (teamAttemptingConversion != null)
                    {
                        // Safeties on conversion attempts end the conversion attempt and do not cause
                        // a free kick.
                        Log.Information("PlayerDownedFunction: Offensive safety on conversion attempt, {Points} points to the defense and kickoff.", safetyPoints);
                        return priorState
                            .WithScoreChange(otherTeam, safetyPoints)
                            .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 35),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} suffers one-point safety!.",
                            DriveResult = DriveResult.TouchdownWithDefensiveScore
                        };
                    }
                    else
                    {
                        Log.Information("PlayerDownedFunction: Safety on standard play, {Points} points to the defense and free kick.", safetyPoints);
                        return priorState
                            .WithScoreChange(otherTeam, safetyPoints)
                            .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.FreeKick,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 20),
                            LineToGain = null,
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} suffers a safety, {DefAbbr} awarded 2 point(s).",
                            DriveResult = DriveResult.Safety
                        };
                    }
                }
                else
                {
                    if (endzoneBehavior == EndzoneBehavior.StandardGameplay
                        || endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
                    {
                        Log.Information("PlayerDownedFunction: Touchdown!");
                        return priorState
                            .WithScoreChange(possessingTeam, 6)
                            .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.ConversionAttempt,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 15),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} touchdown! Scored by {OffPlayer0}."
                        };
                    }
                    else
                    {
                        Log.Information("PlayerDownedFunction: Successful two-point conversion!");
                        return priorState
                            .WithScoreChange(possessingTeam, 2)
                            .WithNextState(PlayEvaluationState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} successful two-point conversion! Scored by {OffPlayer0}.",
                            DriveResult = DriveResult.TouchdownWithTwoPointConversion
                        };
                    }
                }
            }

            // Downed on field normally.
            if (endzoneBehavior == EndzoneBehavior.ConversionAttempt)
            {
                Log.Information("PlayerDownedFunction: Unsuccessful conversion attempt.");
                return priorState.WithNextState(PlayEvaluationState.PlayEvaluationComplete) with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                    ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                    LastPlayDescriptionTemplate =
                        "{OffAbbr} unsuccessful conversion attempt, {OffPlayer0} downed at {LoS}.",
                    DriveResult = DriveResult.TouchdownNoXP
                };
            }
            else if (endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
            {
                Log.Information("PlayerDownedFunction: Fumble or interception return downed in field of play; recovering team is not fumbling team. First down.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, possessingTeam,
                    "{OffAbbr} {OffPlayer0} downed at {LoS} after fumble/interception recovery, first down for {OffAbbr}.",
                    clockRunning, startOfDrive: true);
            }
            else if (priorState.NextPlay == NextPlayKind.FourthDown)
            {
                Log.Information("PlayerDownedFunction: Turnover on downs.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam,
                    // TODO: Ensure that DefAbbr and OffAbbr are correct here - they may need to be swapped.
                    "{DefAbbr} turnover on downs, {DefPlayer0} short of line to gain, first down for {OffAbbr}.", clockRunning,
                    startOfDrive: true) with
                {
                    DriveResult = DriveResult.TurnoverOnDowns
                };
            }
            else if (priorState.NextPlay is NextPlayKind.Kickoff or NextPlayKind.FreeKick)
            {
                // This is where we switch possession after a kickoff or free kick.
                Log.Information("PlayerDownedFunction: Change of possession after kickoff or free kick fielded by receiving team.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam,
                    "{OffAbbr} has first down at {LoS}.", clockRunning, startOfDrive: true);
            }
            else if (priorState.LineToGain == null)
            {
                throw new InvalidOperationException("Cannot compute result of player being downed; code reached path where LineToGain is null but it should not be.");
            }
            else if (priorState.CompareYardForTeam(priorState.LineOfScrimmage, priorState.LineToGain.Value, possessingTeam) >= 0)
            {
                Log.Information("PlayerDownedFunction: First down achieved.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, possessingTeam,
                    "{OffAbbr} {OffPlayer0} has gained a first down at {LoS}.", clockRunning);
            }

            Log.Information("PlayerDownedFunction: Player downed short of line to gain; next down.");
            NextPlayKind nextPlayKind = priorState.NextPlay switch
            {
                NextPlayKind.Kickoff => throw new InvalidOperationException("Unreachable code: Kickoff should have been handled above."),
                NextPlayKind.FirstDown => NextPlayKind.SecondDown,
                NextPlayKind.SecondDown => NextPlayKind.ThirdDown,
                NextPlayKind.ThirdDown => NextPlayKind.FourthDown,
                NextPlayKind.FourthDown => throw new InvalidOperationException("Unreachable code: Turnover on dows should have been handled above."),
                NextPlayKind.ConversionAttempt => throw new InvalidOperationException("Unreachable code: Conversion attempt should have been handled above."),
                NextPlayKind.FreeKick => throw new InvalidOperationException("Unreachable code: Free kick should have been handled above."),
                _ => throw new InvalidOperationException($"Unrecognized NextPlayKind: {priorState.NextPlay}.")
            };
            return priorState.WithNextState(PlayEvaluationState.PlayEvaluationComplete) with
            {
                NextPlay = nextPlayKind,
                LineOfScrimmage = newLineOfScrimmage.Round(),
                PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                ClockRunning = !clockRunning.HasValue ? true : clockRunning.Value,
                LastPlayDescriptionTemplate =
                    "{OffAbbr} {OffPlayer0} downed at {LoS}, {NextPlay}."
            };
        }
    }
}
