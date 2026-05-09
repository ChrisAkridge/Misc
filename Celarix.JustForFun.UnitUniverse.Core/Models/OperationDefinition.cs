using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.UnitUniverse.Core.Models;

public sealed class OperationDefinition
{
    private readonly List<OperationInput> _inputs = new();

    public required string DisplayName { get; init; }
    public required UnitGroup FromGroup { get; init; }
    public required UnitGroup ToGroup { get; init; }
    public required Func<Measurement, IEnumerable<Measurement>> Operation { get; init; }
    public IReadOnlyList<OperationInput> Inputs => _inputs;

    public void Add(OperationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        _inputs.Add(input);
    }
}