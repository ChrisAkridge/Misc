using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
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
            GameTeam? teamAttemptingConversion)
        {
            var possessingTeam = priorState.TeamWithPossession;
            var otherTeam = priorState.OtherTeam(possessingTeam);
            var newLineOfScrimmage = priorState.AddYardsForPossessingTeam(possessionStartYard, yardsGained);
            var gainedTeamYard = priorState.InternalYardToTeamYard(newLineOfScrimmage.Round());

            if (gainedTeamYard.TeamYard < 0)
            {
                if (gainedTeamYard.Team == possessingTeam)
                {
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
                        return priorState.WithScoreChange(otherTeam, safetyPoints) with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 35),
                            LineToGain = null
                        };
                    }
                    else
                    {
                        var updatedState = priorState.WithScoreChange(otherTeam, safetyPoints) with
                        {
                            NextPlay = NextPlayKind.FreeKick,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(possessingTeam, 20),
                            LineToGain = null
                        };
                        return FreeKickDecision.Run(updatedState,
                            parameters,
                            physicsParams);
                    }
                }
                else
                {
                    if (endzoneBehavior == EndzoneBehavior.StandardGameplay)
                    {
                        // Touchdown!
                        return priorState.WithScoreChange(possessingTeam, 6) with
                        {
                            NextPlay = NextPlayKind.ConversionAttempt,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 15),
                            LineToGain = null
                        };
                    }
                    else
                    {
                        // Successful two-point conversion!
                        return priorState.WithScoreChange(possessingTeam, 2) with
                        {
                            NextPlay = NextPlayKind.Kickoff,
                            LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                            LineToGain = null
                        };
                    }
                }
            }

            // Downed on field normally.
            if (endzoneBehavior == EndzoneBehavior.ConversionAttempt)
            {
                // A conversion attempt ended without scoring.
                return priorState with
                {
                    NextPlay = NextPlayKind.Kickoff,
                    LineOfScrimmage = priorState.TeamYardToInternalYard(otherTeam, 35),
                    LineToGain = null
                };
            }
            else if (priorState.NextPlay == NextPlayKind.FourthDown)
            {
                // Turnover on downs.
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam);
            }
            else if (priorState.NextPlay is NextPlayKind.Kickoff or NextPlayKind.FreeKick)
            {
                // This is where we switch possession after a kickoff or free kick.
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, otherTeam);
            }
            else if (priorState.LineToGain == null)
            {
                throw new InvalidOperationException("Cannot compute result of player being downed; code reached path where LineToGain is null but it should not be.");
            }
            else if (priorState.CompareYardForTeam(priorState.LineOfScrimmage, priorState.LineToGain.Value, possessingTeam) >= 0)
            {
                // First down achieved.
                return priorState.WithFirstDownLineOfScrimmage(newLineOfScrimmage, possessingTeam);
            }

            // Normal down progression.
            return priorState with
            {
                NextPlay = priorState.NextPlay switch
                {
                    NextPlayKind.Kickoff => throw new InvalidOperationException("Unreachable code: Kickoff should have been handled above."),
                    NextPlayKind.FirstDown => NextPlayKind.SecondDown,
                    NextPlayKind.SecondDown => NextPlayKind.ThirdDown,
                    NextPlayKind.ThirdDown => NextPlayKind.FourthDown,
                    NextPlayKind.FourthDown => throw new InvalidOperationException("Unreachable code: Turnover on dows should have been handled above."),
                    NextPlayKind.ConversionAttempt => throw new InvalidOperationException("Unreachable code: Conversion attempt should have been handled above."),
                    NextPlayKind.FreeKick => throw new InvalidOperationException("Unreachable code: Free kick should have been handled above."),
                    _ => throw new InvalidOperationException($"Unrecognized NextPlayKind: {priorState.NextPlay}.")
                },
                LineOfScrimmage = newLineOfScrimmage.Round()
            };
        }
    }
}
