using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record GameState(
        // State machine
        long Version,
        GameplayNextState NextState,
        IReadOnlyList<AdditionalParameter<object>> AdditionalParameters,
        IReadOnlyList<StateHistoryEntry> StateHistory,

        // Weather conditions
        double BaseWindDirection,
        double BaseWindSpeed,
        double AirTemperature,

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
        GameplayNextState State,
        GameTeam TeamWithPossession,
        long Version
    );
}
