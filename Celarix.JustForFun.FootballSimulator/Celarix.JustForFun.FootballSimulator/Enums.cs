using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator;

internal enum GameResultForTeam
{
    Win,
    Loss,
    Tie
}

internal enum NextPlayKind
{
    Kickoff,
    FirstDown,
    SecondDown,
    ThirdDown,
    FourthDown,
    ConversionAttempt,
    FreeKick
}

internal enum PlayResultKind
{
    BallDead,
    IncompletePass,
    FieldGoal,
    MissedFieldGoal,
    Touchdown,
    ConversionAttempt,
    Safety,
    PuntDownedByPuntingTeam
}

internal enum DriveDirection
{
    // Toward internal yard 0
    TowardHomeEndzone,
    // Toward internal yard 100
    TowardAwayEndzone
}