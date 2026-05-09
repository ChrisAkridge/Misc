using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.UnitUniverse.Core.Models;

public sealed record Unit
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Abbreviation { get; init; }
    public required Dimension Dimension { get; init; }
    public required bool IsCanonical { get; init; }
    public required double ConversionFactorToCanonical { get; init; }
    public required Func<double, string> Formatter { get; init; }
}