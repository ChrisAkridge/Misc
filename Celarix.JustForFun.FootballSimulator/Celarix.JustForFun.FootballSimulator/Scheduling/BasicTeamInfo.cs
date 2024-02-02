using System;
using System.Collections.Generic;
using System.Linq;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal sealed class BasicTeamInfo : IComparable<BasicTeamInfo>
{
    public string Name { get; }
    public Conference Conference { get; }
    public Division Division { get; }

    public BasicTeamInfo(string name, Conference conference, Division division)
    {
        Name = name;
        Conference = conference;
        Division = division;
    }

    /// <summary>Indicates whether this instance and a specified object are equal.</summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object? obj) => obj is BasicTeamInfo other && Name == other.Name;

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
    public override int GetHashCode() => Name.GetHashCode();

    /// <summary>Returns the fully qualified type name of this instance.</summary>
    /// <returns>The fully qualified type name.</returns>
    public override string ToString() => Name;

    /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
    /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
    public int CompareTo(BasicTeamInfo? other) => other == null
            ? 1
            : string.Compare(Name, other.Name, StringComparison.Ordinal);
}