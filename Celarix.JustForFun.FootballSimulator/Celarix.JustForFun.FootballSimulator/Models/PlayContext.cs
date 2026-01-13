using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record PlayContext(
        // State machine
        long Version,
        PlayEvaluationState NextState,
        IReadOnlyList<AdditionalParameter<object>> AdditionalParameters,
        IReadOnlyList<StateHistoryEntry> StateHistory,

        // Environment
        PlayEnvironment? Environment,

        // Initial conditions
        double BaseWindDirection,
        double BaseWindSpeed,
        double AirTemperature,
        GameTeam CoinFlipWinner,

        // Game status
        GameTeam TeamWithPossession,
        int AwayScore,
        int HomeScore,
        int PeriodNumber,
        int SecondsLeftInPeriod,
        bool ClockRunning,
        int HomeTimeoutsRemaining,
        int AwayTimeoutsRemaining,

        // Down-and-distance
        int LineOfScrimmage,
        int? LineToGain,
        NextPlayKind NextPlay,

        // Start-of-drive properties
        int DriveStartingFieldPosition,
        int DriveStartingPeriodNumber,
        int DriveStartingSecondsLeftInPeriod,

        // Display properties
        string LastPlayDescriptionTemplate,

        // Internal properties
        PossessionOnPlay PossessionOnPlay,
        GameTeam? TeamCallingTimeout
    );

    internal sealed record AdditionalParameter<T>(
        string Key,
        T Value,
        long AddedInVersion
    );

    internal sealed record StateHistoryEntry(
        PlayEvaluationState State,
        GameTeam TeamWithPossession,
        long Version
    );
}
