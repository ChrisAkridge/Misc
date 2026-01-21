using System;
using System.Collections.Generic;
using System.Text;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.SummaryWriting
{
    internal interface ISummaryWriter
    {
        string WriteGameSummary(GameRecord gameRecord);
        string WriteSeasonSummary(SeasonRecord seasonRecord, IReadOnlyList<Team> teams);
    }
}
