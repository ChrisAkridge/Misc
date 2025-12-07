using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator;

public enum CurrentSystemState
{
    /// <summary>
    /// Represents a state where the system is either initializing for the first time, or is checking
    /// where to continue from, given the current data.
    /// </summary>
    Initialization,

    /// <summary>
    /// Represents a state where the system is initializing a new game, including selecting the next
    /// game, initializing parameters, and doing the coin toss.
    /// </summary>
    GameInitialization,

    /// <summary>
    /// Represents the state where standard gameplay is occurring, including plays, drives, and possessions.
    /// </summary>
    StandardGameplay,

    /// <summary>
    /// Represents the state at the end of each quarter and overtime period.
    /// </summary>
    EndOfQuarter,

    /// <summary>
    /// Represents the state where all regular season games are concluded and the playoffs are being
    /// initialized.
    /// </summary>
    PlayoffInitialization,

    /// <summary>
    /// Represents the state after the playoffs, where the system runs a draft, makes trades, and generates
    /// the schedule for the next season.
    /// </summary>
    Offseason,

    /// <summary>
    /// Represents the state where an unrecoverable error has occurred and the system cannot continue normal operation.
    /// </summary>
    Faulted
}

public enum ScheduledGameType
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

public enum DriveDirection
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

internal enum StrengthSetKind
{
    TeamOfItself,
    TeamOfOpponent
}

internal enum EndzoneBehavior
{
    /// <summary>
    /// Being downed in the opponent's endzone is a touchdown, your own is a safety.
    /// </summary>
    StandardGameplay,

    /// <summary>
    /// Being downed in the opponent's endzone results in a successful two-point conversion, your own is a safety.
    /// </summary>
    ConversionAttempt
}