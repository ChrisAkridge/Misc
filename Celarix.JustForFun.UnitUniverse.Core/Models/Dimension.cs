using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.UnitUniverse.Core.Models;

public sealed record Dimension
{
    public IReadOnlyDictionary<BaseDimension, int> Exponents { get; }
}