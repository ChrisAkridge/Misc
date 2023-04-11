using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using ArgumentOutOfRangeException = System.ArgumentOutOfRangeException;
using static Celarix.JustForFun.FootballSimulator.Helpers;

namespace Celarix.JustForFun.FootballSimulator
{
    internal static class NextPlayComputer
    {
        public static NextPlay DetermineNextPlay(PlayResult lastPlayResult)
        {
            switch (lastPlayResult.Kind)
            {
                case PlayResultKind.ConversionAttempt or PlayResultKind.FieldGoal:
                    return new NextPlay
                    {
                        Kind = NextPlayKind.Kickoff,
                        Team = lastPlayResult.Team,
                        LineOfScrimmage = TeamYardLineToInternalYardLine(35, lastPlayResult.Team),
                        FirstDownLine = null,
                        Direction = TowardOpponentEndzone(lastPlayResult.Team)
                    };
                case PlayResultKind.MissedFieldGoal:
                {
                    var ballDeadYard = lastPlayResult.BallDeadYard
                        ?? throw new ArgumentNullException(nameof(lastPlayResult.BallDeadYard),
                            "Cannot have a null ball-dead-yard for a missed field goal kick");
                    var direction = TowardOpponentEndzone(lastPlayResult.Team);

                    return new NextPlay
                    {
                        Kind = NextPlayKind.FirstDown,
                        Team = OtherTeam(lastPlayResult.Team),
                        LineOfScrimmage = ballDeadYard,
                        FirstDownLine = YardsDownfield(ballDeadYard, 10, direction),
                        Direction = direction
                    };
                }
                case PlayResultKind.Safety:
                    return new NextPlay
                    {
                        Kind = NextPlayKind.FreeKick,
                        Team = lastPlayResult.Team,
                        LineOfScrimmage = TeamYardLineToInternalYardLine(20, lastPlayResult.Team),
                        FirstDownLine = null,
                        Direction = TowardOpponentEndzone(lastPlayResult.Team)
                    };
                case PlayResultKind.BallDead or PlayResultKind.IncompletePass:
                {
                    var ballDeadYard = lastPlayResult.BallDeadYard
                        ?? throw new ArgumentNullException(nameof(lastPlayResult.BallDeadYard),
                            "Cannot have a null ball-dead-yard for a ball dead or incomplete pass play result.");

                    if (lastPlayResult.Kind == PlayResultKind.BallDead)
                    {
                        var firstDownAchieved = lastPlayResult.DownNumber == null
                            || IsYardLineBeyondYardLine(ballDeadYard,
                                lastPlayResult.FirstDownLine
                                ?? throw new ArgumentNullException(nameof(lastPlayResult.FirstDownLine),
                                    "Cannot have a null first-down line when the play result isn't Touchdown."),
                                lastPlayResult.Direction);

                        if (firstDownAchieved)
                        {
                            return new NextPlay
                            {
                                Kind = NextPlayKind.FirstDown,
                                Team = lastPlayResult.Team,
                                Direction = lastPlayResult.Direction,
                                LineOfScrimmage = ballDeadYard,
                                FirstDownLine = YardsDownfield(ballDeadYard, 10, lastPlayResult.Direction),
                            };
                        }
                    }
                
                    switch (lastPlayResult.DownNumber)
                    {
                        case 4:
                        {
                            var otherTeamDirection = TowardOpponentEndzone(OtherTeam(lastPlayResult.Team));

                            return new NextPlay
                            {
                                Kind = NextPlayKind.FirstDown,
                                Team = OtherTeam(lastPlayResult.Team),
                                Direction = otherTeamDirection,
                                LineOfScrimmage = ballDeadYard,
                                FirstDownLine = YardsDownfield(ballDeadYard, 10, otherTeamDirection)
                            };
                        }
                        case null:
                        {
                            // Handles kickoffs and touchbacks
                            var otherTeamDirection = TowardOpponentEndzone(OtherTeam(lastPlayResult.Team));
                            var newLineOfScrimmage = lastPlayResult.BallDeadYard
                                ?? TeamYardLineToInternalYardLine(25, OtherTeam(lastPlayResult.Team));
                    
                            return new NextPlay
                            {
                                Kind = NextPlayKind.FirstDown,
                                Team = OtherTeam(lastPlayResult.Team),
                                Direction = otherTeamDirection,
                                LineOfScrimmage = newLineOfScrimmage,
                                FirstDownLine = YardsDownfield(newLineOfScrimmage, 10, otherTeamDirection)
                            };
                        }
                        default:
                            return new NextPlay
                            {
                                Kind = lastPlayResult.DownNumber switch
                                {
                                    1 => NextPlayKind.SecondDown,
                                    2 => NextPlayKind.ThirdDown,
                                    3 => NextPlayKind.FourthDown,
                                    _ => throw new ArgumentOutOfRangeException(nameof(lastPlayResult.DownNumber))
                                },
                                Team = lastPlayResult.Team,
                                Direction = lastPlayResult.Direction,
                                LineOfScrimmage = ballDeadYard,
                                FirstDownLine = lastPlayResult.FirstDownLine
                            };
                    }
                }
                case PlayResultKind.PuntDownedByPuntingTeam:
                {
                    var otherTeamDirection = TowardOpponentEndzone(OtherTeam(lastPlayResult.Team));

                    var newLineOfScrimmage = lastPlayResult.BallDeadYard ?? TeamYardLineToInternalYardLine(25, OtherTeam(lastPlayResult.Team));

                    return new NextPlay
                    {
                        Kind = NextPlayKind.FirstDown,
                        Team = OtherTeam(lastPlayResult.Team),
                        LineOfScrimmage = newLineOfScrimmage,
                        Direction = otherTeamDirection,
                        FirstDownLine = YardsDownfield(newLineOfScrimmage, 10, otherTeamDirection)
                    };
                }
                case PlayResultKind.Touchdown:
                    return new NextPlay
                    {
                        Direction = lastPlayResult.Direction,
                        FirstDownLine = null,
                        Kind = NextPlayKind.ConversionAttempt,
                        LineOfScrimmage = 15, // can be changed to the 2-yard-line if the team decides to go for 2
                        Team = lastPlayResult.Team
                    };
                case PlayResultKind.KickRecovered:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(lastPlayResult.Kind), $"Kind of last play result {lastPlayResult.Kind} is not valid.");
            }
        }
    }
}
