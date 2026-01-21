using Celarix.JustForFun.FootballSimulator.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class PostGameStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var gameRecord = context.Environment.CurrentGameRecord!;
            var repository = context.Environment.FootballRepository;
            gameRecord.GameComplete = true;

            // Mark any claimed injury recoveries as claimed so we can't recover them again.
            // We use the same logic to get them here as we do when loading a game; this way,
            // if the app restarts mid-game, we don't have to remember what injuries we already recovered.
            var claimedRecoveries = repository.GetInjuryRecoveriesForGame(gameRecord.AwayTeamID,
                gameRecord.HomeTeamID,
                gameRecord.KickoffTime);
            foreach (var recovery in claimedRecoveries)
            {
                recovery.Recovered = true;
            }

            repository.SaveChanges();

            var wasSuperBowl = gameRecord.WeekNumber == 22;
            return context.WithNextState(wasSuperBowl ? SystemState.WriteSummaryForSeason : SystemState.WriteSummaryForGame);
        }
    }
}
