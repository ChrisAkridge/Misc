using Celarix.JustForFun.FootballSimulator.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.SummaryWriting
{
    internal class FallbackSummaryWriter : ISummaryWriter
    {
        public string WriteGameSummary(GameRecord gameRecord)
        {
            throw new NotImplementedException();
        }

        public string WriteSeasonSummary(SeasonRecord seasonRecord, IReadOnlyList<Team> teams)
        {
            throw new NotImplementedException();
        }
    }
}
