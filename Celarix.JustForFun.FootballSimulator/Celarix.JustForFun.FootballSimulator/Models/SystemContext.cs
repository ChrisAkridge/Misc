using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record SystemContext(
        // State machine
        long Version,
        SystemState NextState,

        // Environment
        SystemEnvironment Environment
    );
}
