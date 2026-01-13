using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record GameContext(
        // State machine
        long Version,
        GameState NextState,

        // Environment
        GameEnvironment Environment
    );
}
