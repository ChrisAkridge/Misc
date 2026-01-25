using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core
{
    internal static class PlayContextExtensions
    {
        extension(PlayContext state)
        {
            public PlayContext WithNextState(PlayEvaluationState nextState)
            {
                return state with
                {
                    NextState = nextState,
                    StateHistory = ImmutableList.CreateRange(state.StateHistory
                        .Append(new StateHistoryEntry(state.NextState, state.TeamWithPossession, state.Version)))
                };
            }

            public PlayContext WithAdditionalParameter<T>(string key, T value)
            {
                return state with
                {
                    AdditionalParameters = ImmutableList.CreateRange(state.AdditionalParameters
                        .Append(new AdditionalParameter<object>(key, value!, state.Version)))
                };
            }

            public bool HasAdditionalParameter(string key)
            {
                if (state.AdditionalParameters != null)
                {
                    var match = state.AdditionalParameters.SingleOrDefault(p => p.Key == key && p.AddedInVersion >= state.Version - 1);
                    return match != null;
                }
                return false;
            }

            public T? GetAdditionalParameterOrDefault<T>(string key)
            {
                if (state.AdditionalParameters != null)
                {
                    var match = state.AdditionalParameters.SingleOrDefault(p => p.Key == key && p.AddedInVersion >= state.Version - 1);

                    if (match != null)
                    {
                        return (T)match.Value;
                    }
                }
                return default;
            }

            public int GetScoreForTeam(GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => state.AwayScore,
                    GameTeam.Home => state.HomeScore,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public int GetScoreDifferenceForTeam(GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => state.AwayScore - state.HomeScore,
                    GameTeam.Home => state.HomeScore - state.AwayScore,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public int TotalSecondsLeftInGame()
            {
                if (state.PeriodNumber > 4)
                {
                    // Overtime - assume 10-minute periods
                    return state.SecondsLeftInPeriod;
                }
                else
                {
                    var periodsLeft = 4 - state.PeriodNumber;
                    return state.SecondsLeftInPeriod + (periodsLeft * Constants.SecondsPerQuarter);
                }
            }

            public double GetFacingEndzoneAngle(GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => 0,
                    GameTeam.Home => 180,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public double AddYardsForPossessingTeam(double startYard, double addend)
            {
                // For the away team, they're trying to get to internal yard 0, so adding yards here means subtracting from startYard
                // For the home team, they're trying to get to internal yard 100, so adding yards here means adding to startYard
                return AddYardsForTeam(startYard, addend, state.TeamWithPossession);
            }

            public double DistanceForPossessingTeam(double fromYard, double toYard)
            {
                // For the away team, they're trying to get to internal yard 0, so distance is fromYard - toYard
                // For the home team, they're trying to get to internal yard 100, so distance is toYard - fromYard
                return state.TeamWithPossession switch
                {
                    GameTeam.Away => fromYard - toYard,
                    GameTeam.Home => toYard - fromYard,
                    _ => throw new ArgumentOutOfRangeException(nameof(state.TeamWithPossession), $"Unhandled team value: {state.TeamWithPossession}")
                };
            }

            public int TeamYardToInternalYard(GameTeam team, int teamYard)
            {
                if (teamYard < -10d || teamYard > 50d)
                {
                    throw new ArgumentOutOfRangeException(nameof(teamYard), $"teamYard must be between -10 and 50, inclusive. Value provided: {teamYard}");
                }

                return team switch
                {
                    GameTeam.Away => 100 - teamYard,
                    GameTeam.Home => teamYard,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public (GameTeam Team, int TeamYard) InternalYardToTeamYard(int internalYard)
            {
                if (internalYard < Constants.HomeEndLineYard || internalYard > Constants.AwayEndLineYard)
                {
                    throw new ArgumentOutOfRangeException(nameof(internalYard), $"internalYard must be between -10 and 110, inclusive. Value provided: {internalYard}");
                }
                if (internalYard <= 50)
                {
                    return (GameTeam.Home, internalYard);
                }
                else
                {
                    return (GameTeam.Away, 100 - internalYard);
                }
            }

            public string InternalYardToDisplayTeamYardString(int internalYard, GameDecisionParameters parameters)
            {
                var (team, teamYard) = state.InternalYardToTeamYard(internalYard);
                var teamAbbreviation = team switch
                {
                    GameTeam.Away => parameters.AwayTeam.Abbreviation,
                    GameTeam.Home => parameters.HomeTeam.Abbreviation,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };

                return $"{teamAbbreviation} {teamYard}";
            }

            public PlayContext WithScoreChange(GameTeam scoringTeam, int points)
            {
                return scoringTeam switch
                {
                    GameTeam.Away => state with { AwayScore = state.AwayScore + points },
                    GameTeam.Home => state with { HomeScore = state.HomeScore + points },
                    _ => throw new ArgumentOutOfRangeException(nameof(scoringTeam), $"Unhandled team value: {scoringTeam}")
                };
            }

            public PlayContext WithFirstDownLineOfScrimmage(double newLineOfScrimmage, GameTeam team, string lastPlayDescriptionTemplate,
                bool? clockRunning = null, bool startOfDrive = false)
            {
                var teamYard = InternalYardToTeamYard(state, newLineOfScrimmage.Round());
                if (teamYard.TeamYard is <= 0 && teamYard.Team == team.Opponent())
                {
                    // Did not return ball out of opponent's endzone, touchback.
                    return state.WithFirstDownLineOfScrimmage(state.TeamYardToInternalYard(team, 20), team, lastPlayDescriptionTemplate, clockRunning,
                        startOfDrive: true);
                }

                var desiredLineToGain = AddYardsForTeam(newLineOfScrimmage, 10, team);
                var desiredTeamLineToGain = InternalYardToTeamYard(state, desiredLineToGain.Round());
                if (desiredTeamLineToGain.TeamYard < 0)
                {
                    desiredLineToGain = team switch
                    {
                        GameTeam.Away => Constants.HomeGoalLineYard,
                        GameTeam.Home => Constants.AwayGoalLineYard,
                        _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                    };
                }

                return (state.WithNextState(PlayEvaluationState.PlayEvaluationComplete) with
                {
                    TeamWithPossession = team,
                    PossessionOnPlay = team.ToPossessionOnPlay(),
                    NextPlay = NextPlayKind.FirstDown,
                    LineOfScrimmage = newLineOfScrimmage.Round(),
                    LineToGain = desiredLineToGain.Round(),
                    LastPlayDescriptionTemplate = lastPlayDescriptionTemplate
                })
                .StartDrive(startOfDrive);
            }

            public int CompareYardForTeam(int yardA, int yardB, GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => yardB.CompareTo(yardA),
                    GameTeam.Home => yardA.CompareTo(yardB),
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public int CompareYardForTeam(double yardA, double yardB, GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => yardB.CompareTo(yardA),
                    GameTeam.Home => yardA.CompareTo(yardB),
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            internal static double AddYardsForTeam(double startYard, double addend, GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => startYard - addend,
                    GameTeam.Home => startYard + addend,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public int TimeoutsRemainingForTeam(GameTeam team)
            {
                return team switch
                {
                    GameTeam.Away => state.AwayTimeoutsRemaining,
                    GameTeam.Home => state.HomeTimeoutsRemaining,
                    _ => throw new ArgumentOutOfRangeException(nameof(team), $"Unhandled team value: {team}")
                };
            }

            public int? DistanceToGo()
            {
                if (state.LineToGain.HasValue)
                {
                    return state.DistanceForPossessingTeam(state.LineOfScrimmage, state.LineToGain.Value).Round();
                }
                else
                {
                    return null;
                }
            }

            public int DistanceToEndzone()
            {
                return state.TeamWithPossession switch
                {
                    GameTeam.Away => state.LineOfScrimmage,
                    GameTeam.Home => (100 - state.LineOfScrimmage),
                    _ => throw new ArgumentOutOfRangeException(nameof(state.TeamWithPossession), $"Unhandled team value: {state.TeamWithPossession}")
                };
            }

            public PlayContext TakeTimeout(GameTeam team)
            {
                return state.WithNextState(PlayEvaluationState.PlayEvaluationComplete) with
                {
                    AwayTimeoutsRemaining = team == GameTeam.Away ? state.AwayTimeoutsRemaining - 1 : state.AwayTimeoutsRemaining,
                    HomeTimeoutsRemaining = team == GameTeam.Home ? state.HomeTimeoutsRemaining - 1 : state.HomeTimeoutsRemaining,
                    TeamCallingTimeout = team,
                    ClockRunning = false,
                    LastPlayDescriptionTemplate = $"{(team == GameTeam.Away ? "Away team" : "Home team")} called a timeout."
                };
            }

            public string GetDescription(GameDecisionParameters parameters)
            {
                const char possessionSymbol = '⬭';
                var sb = new StringBuilder();

                if (state.TeamWithPossession == GameTeam.Away)
                {
                    sb.Append(parameters.AwayTeam.Abbreviation + ' ');
                    sb.Append(possessionSymbol);
                }
                else
                {
                    sb.Append(parameters.AwayTeam.Abbreviation + ' ');
                }

                sb.Append(' ');
                sb.Append(state.AwayScore);
                sb.Append(" - ");
                sb.Append(state.HomeScore + ' ');

                if (state.TeamWithPossession == GameTeam.Home)
                {
                    sb.Append(possessionSymbol);
                    sb.Append(' ' + parameters.HomeTeam.Abbreviation);
                }
                else
                {
                    sb.Append(' ' + parameters.HomeTeam.Abbreviation);
                }

                sb.Append(", ");

                sb.Append(state.NextPlay switch
                {
                    NextPlayKind.FirstDown => "1st and ",
                    NextPlayKind.SecondDown => "2nd and ",
                    NextPlayKind.ThirdDown => "3rd and ",
                    NextPlayKind.FourthDown => "4th and ",
                    NextPlayKind.Kickoff => "Kickoff ",
                    NextPlayKind.ConversionAttempt => "XPA ",
                    NextPlayKind.FreeKick => "Free Kick ",
                    _ => throw new InvalidOperationException($"Unexpected NextPlayKind value: {state.NextPlay}")
                });

                if (state.LineToGain != null)
                {
                    var distanceToGain = state.DistanceForPossessingTeam(state.LineOfScrimmage, state.LineToGain.Value).Round();
                    if (distanceToGain == 0)
                    {
                        sb.Append("inches");
                    }
                    else
                    {
                        sb.Append(distanceToGain);
                    }
                }
                else if (state.NextPlay is NextPlayKind.FirstDown or NextPlayKind.SecondDown or NextPlayKind.ThirdDown or NextPlayKind.FourthDown)
                {
                    sb.Append("Goal");
                }

                sb.Append(" @ ");
                sb.Append(state.InternalYardToDisplayTeamYardString(state.LineOfScrimmage, parameters) + ", ");

                if (state.PeriodNumber <= 4)
                {
                    sb.Append($"Q{state.PeriodNumber} ");
                }
                else if (state.PeriodNumber == 5)
                {
                    sb.Append("OT ");
                }
                else
                {
                    sb.Append($"{state.PeriodNumber - 4}OT ");
                }

                var minutes = state.SecondsLeftInPeriod / 60;
                var seconds = state.SecondsLeftInPeriod % 60;
                sb.Append($"{minutes}:{seconds:D2}");

                return sb.ToString();
            }

            public PlayContext StartDrive(bool startOfDrive)
            {
                return state with
                {
                    DriveStartingFieldPosition = startOfDrive ? state.LineOfScrimmage : state.DriveStartingFieldPosition,
                    DriveStartingPeriodNumber = startOfDrive ? state.PeriodNumber : state.DriveStartingPeriodNumber,
                    DriveStartingSecondsLeftInPeriod = startOfDrive ? state.SecondsLeftInPeriod : state.DriveStartingSecondsLeftInPeriod
                };
            }

            // PlayInvolvement Helpers
            public PlayContext InvolvesOffensiveRun()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        InvolvesOffenseRun = true
                    }
                };
            }

            public PlayContext InvolvesOffensivePass()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        InvolvesOffensePass = true
                    }
                };
            }

            public PlayContext InvolvesKick()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        InvolvesKick = true
                    }
                };
            }

            public PlayContext InvolvesDefenseRun()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        InvolvesDefenseRun = true
                    }
                };
            }

            public PlayContext InvolvesAdditionalOffensivePlayer()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        OffensivePlayersInvolved = state.PlayInvolvement.OffensivePlayersInvolved + 1
                    }
                };
            }

            public PlayContext InvolvesAdditionalDefensivePlayer()
            {
                return state with
                {
                    PlayInvolvement = state.PlayInvolvement with
                    {
                        DefensivePlayersInvolved = state.PlayInvolvement.DefensivePlayersInvolved + 1
                    }
                };
            }
        }
    }
}
