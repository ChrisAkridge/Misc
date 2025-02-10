using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator;

internal enum ScheduledGameType
{
    IntradivisionalFirstSet,
    IntradivisionalSecondSet,
    IntraconferenceFirstSet,
    IntraconferenceSecondSet,
    Interconference,
    RemainingIntraconferenceFirstSet,
    RemainingIntraconferenceSecondSet,
}

internal enum GameWeekSlotType
{
	Empty,
	Assigned,
	PreviouslyAssigned,
	Ineligible
}

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
    PuntDownedByPuntingTeam,
    KickRecovered
}

internal enum DriveDirection
{
    // Toward internal yard 0
    TowardHomeEndzone,
    // Toward internal yard 100
    TowardAwayEndzone
}

internal enum ClockEvent
{
    TimeElapsed,
    EndOfQuarter,
    EndOfHalf,
    EndOfGame,
    StartingOvertimePeriod,
    NewCoinTossRequired
}