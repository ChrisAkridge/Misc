using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed record RushingResult(
        bool WasFumbled,
        double? YardsGained
    );
}
