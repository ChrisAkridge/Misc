using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed record SystemContext(
        // State machine
        long Version,
        SystemState NextState,

        // Environment
        [property: JsonIgnore] SystemEnvironment Environment
    );
}
