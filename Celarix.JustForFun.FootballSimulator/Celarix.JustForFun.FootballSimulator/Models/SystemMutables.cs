using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    internal sealed class SystemMutables
    {
        public required FootballContext FootballContext { get; init; }
        public required IRandomFactory RandomFactory { get; init; }
        public GameRecord? CurrentGameRecord { get; set; }
    }
}
