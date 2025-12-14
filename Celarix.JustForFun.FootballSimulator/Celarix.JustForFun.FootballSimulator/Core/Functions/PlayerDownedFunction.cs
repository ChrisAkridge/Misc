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
        public static GameState Get(GameState priorState,
            GameDecisionParameters parameters,
            IReadOnlyDictionary<string, PhysicsParam> physicsParams,
            int possessionStartYard,
            int yardsGained,
            EndzoneBehavior endzoneBehavior,
            GameTeam? teamAttemptingConversion,
            bool? clockRunning = null)
        {
            var possessingTeam = priorState.TeamWithPossession;
            var otherTeam = possessingTeam.Opponent();
            var newLineOfScrimmage = priorState.AddYardsForPossessingTeam(possessionStartYard, yardsGained);
            var gainedTeamYard = priorState.InternalYardToTeamYard(newLineOfScrimmage.Round());
            string displayYard = priorState.InternalYardToDisplayTeamYardString(newLineOfScrimmage.Round(), parameters);

            if (gainedTeamYard.TeamYard < 0)
            {
                if (gainedTeamYard.Team == possessingTeam)
                {
                    if (endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
                    {
                        Log.Verbose("PlayerDownedFunction: Fumble recovered in own endzone, touchback.");
                        return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
                        {
                            NextPlay = NextPlayKind.FirstDown,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 25),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
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
                        Log.Verbose("PlayerDownedFunction: Offensive safety on conversion attempt, 2 points to the defense and kickoff.");
                        return priorState
                            .WithScoreChange(otherTeam, safetyPoints)
                            .WithNextState(GameplayNextState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 35),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} suffers one-point safety!."
                        };
                    }
                    else
                    {
                        Log.Verbose("PlayerDownedFunction: Safety on standard play, 2 points to the defense and free kick.");
                        return priorState
                            .WithScoreChange(otherTeam, safetyPoints)
                            .WithNextState(GameplayNextState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.FreeKick,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 20),
                            LineToGain = null,
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} suffers a safety, {DefAbbr} awarded 2 point(s)."
                        };
                    }
                }
                else
                {
                    if (endzoneBehavior == EndzoneBehavior.StandardGameplay
                        || endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
                    {
                        Log.Verbose("PlayerDownedFunction: Touchdown!");
                        return priorState
                            .WithScoreChange(possessingTeam, 6)
                            .WithNextState(GameplayNextState.PlayEvaluationComplete)
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
                        Log.Verbose("PlayerDownedFunction: Successful two-point conversion!");
                        return priorState
                            .WithScoreChange(possessingTeam, 2)
                            .WithNextState(GameplayNextState.PlayEvaluationComplete)
                        with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                            LineToGain = null,
                            PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                            ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                            LastPlayDescriptionTemplate = "{OffAbbr} successful two-point conversion! Scored by {OffPlayer0}."
                        };
                    }
                }
            }

            // Downed on field normally.
            if (endzoneBehavior == EndzoneBehavior.ConversionAttempt)
            {
                Log.Verbose("PlayerDownedFunction: Unsuccessful conversion attempt.");
                return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                    LineToGain = null,
                    PossessionOnPlay = possessingTeam.ToPossessionOnPlay(),
                    ClockRunning = !clockRunning.HasValue ? false : clockRunning.Value,
                    LastPlayDescriptionTemplate =
                        "{OffAbbr} unsuccessful conversion attempt, {OffPlayer0} downed at {LoS}."
                };
            }
            else if (endzoneBehavior == EndzoneBehavior.FumbleOrInterceptionReturn)
            {
                Log.Verbose("PlayerDownedFunction: Fumble or interception return downed in field of play; recovering team is not fumbling team. First down.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, possessingTeam,
                    "{OffAbbr} {OffPlayer0} downed at {LoS} after fumble/interception recovery, first down for {OffAbbr}.",
                    clockRunning);
            }
            else if (priorState.NextPlay == NextPlayKind.FourthDown)
            {
                Log.Verbose("PlayerDownedFunction: Turnover on downs.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam,
                    // TODO: Ensure that DefAbbr and OffAbbr are correct here - they may need to be swapped.
                    "{DefAbbr} turnover on downs, {DefPlayer0} short of line to gain, first down for {OffAbbr}.", clockRunning);
            }
            else if (priorState.NextPlay is NextPlayKind.Kickoff or NextPlayKind.FreeKick)
            {
                // This is where we switch possession after a kickoff or free kick.
                Log.Verbose("PlayerDownedFunction: Change of possession after kickoff or free kick fielded by receiving team.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam,
                    "{OffAbbr} has first down at {LoS}.", clockRunning);
            }
            else if (priorState.LineToGain == null)
            {
                throw new InvalidOperationException("Cannot compute result of player being downed; code reached path where LineToGain is null but it should not be.");
            }
            else if (priorState.CompareYardForTeam(priorState.LineOfScrimmage, priorState.LineToGain.Value, possessingTeam) >= 0)
            {
                Log.Verbose("PlayerDownedFunction: First down achieved.");
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, possessingTeam,
                    "{OffAbbr} {OffPlayer0} has gained a first down at {LoS}.", clockRunning);
            }

            Log.Verbose("PlayerDownedFunction: Player downed short of line to gain; next down.");
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
            return priorState.WithNextState(GameplayNextState.PlayEvaluationComplete) with
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
