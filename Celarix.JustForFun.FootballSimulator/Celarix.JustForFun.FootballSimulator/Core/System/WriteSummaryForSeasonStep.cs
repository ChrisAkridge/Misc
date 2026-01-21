using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.SummaryWriting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class WriteSummaryForSeasonStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var summaryWriter = context.Environment.SummaryWriter;
            var currentSeasonID = context.Environment.CurrentGameRecord!.SeasonRecordID;
            var currentSeason = repository.GetSeasonWithGames(currentSeasonID);
            var allTeams = repository.GetTeams();

            // Write season summary
            var seasonSummaryText = summaryWriter.WriteSeasonSummary(currentSeason, allTeams);
            var summary = new Summary
            {
                SeasonRecordID = currentSeasonID,
                SummaryText = seasonSummaryText
            };
            repository.AddSummary(summary);

            // Handle trades
            var physicsParams = repository.GetPhysicsParams();

            // 1. Get all rosters for all teams
            // 2. For each team, pick a random number of trades to make ("TeamPlayersTradedPerOffseasonMean" and "TeamPlayersTradedPerOffseasonStdDev")
            // 3. Pick that many players at random from the roster and add them to a trade pool
            // 4. End all PlayerRosterPositions for those players
            // 5. While any team has fewer than 23 players, pick a random team with fewer than 23 players and give them a random player from the trade pool
            // 6. Make a new PlayerRosterPosition for that player on the new team
            // 7. Repeat until all teams have at least 23 players
            // 8. If any players remain in the trade pool, retire them by not making a new PlayerRosterPosition for them
            // 9. If the trade pool runs out instead, draft new players to fill rosters up to 23 players

            // Mark season as complete
            currentSeason.SeasonComplete = true;
            repository.SaveChanges();
            return context.WithNextState(SystemState.PrepareForGame);
        }
    }
}
