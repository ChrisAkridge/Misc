using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class WriteSummaryForGameStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var gameRecord = context.Environment.CurrentGameRecord!;
            var repository = context.Environment.FootballRepository;
            var summaryWriter = context.Environment.SummaryWriter;

            var summaryText = summaryWriter.WriteGameSummary(gameRecord);
            var summary = new Summary
            {
                GameRecordID = gameRecord.GameID,
                SeasonRecordID = gameRecord.SeasonRecordID,
                SummaryText = summaryText
            };
            repository.AddSummary(summary);
            repository.SaveChanges();

            return context.WithNextState(SystemState.PrepareForGame);
        }
    }
}
