using Celarix.JustForFun.UnitUniverse.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.UnitUniverse.Core.Models;

public sealed class UnitGroup
{
    private readonly List<Unit> _units = new();

    public required string Name { get; init; }
    public IReadOnlyList<Unit> Units => _units;
    public Unit? CanonicalUnit { get; private set; }

    public void Add(Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        _units.Add(unit);
    }

    public void SetCanonicalUnit(string unitName)
    {
        var canonicalUnit = _units.Find(u => u.Name == unitName);
        if (canonicalUnit is null)
        {
            throw new ArgumentException($"No unit with name '{unitName}' found in the group.", nameof(unitName));
        }
        CanonicalUnit = canonicalUnit;
    }
}