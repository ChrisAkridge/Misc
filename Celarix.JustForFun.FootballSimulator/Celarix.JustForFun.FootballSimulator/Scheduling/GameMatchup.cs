using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class GameMatchup
{
    public BasicTeamInfo AwayTeam { get; init; }
    public BasicTeamInfo HomeTeam { get; init; }
    public int GameType { get; init; }
    public BasicTeamInfo AddedBy { get; init; }
    public Guid GamePairId { get; init; }

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj) =>
        obj is GameMatchup that
        && AwayTeam.Equals(that.AwayTeam)
        && HomeTeam.Equals(that.HomeTeam)
        && GameType == that.GameType;

    public bool SymmetricallyEquals(GameMatchup that) =>
        (AwayTeam.Equals(that.AwayTeam) || AwayTeam.Equals(that.HomeTeam))
        && (HomeTeam.Equals(that.HomeTeam) || HomeTeam.Equals(that.AwayTeam))
        && (GameType == that.GameType);

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => HashCode.Combine(AwayTeam, HomeTeam, GameType);

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{AwayTeam} @ {HomeTeam} (type {GameType}, added by {AddedBy})";
}