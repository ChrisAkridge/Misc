using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public sealed class SystemStatus
    {
        public CurrentSystemState CurrentState { get; init; }
        public required string StatusMessage { get; init; }
    }
}
