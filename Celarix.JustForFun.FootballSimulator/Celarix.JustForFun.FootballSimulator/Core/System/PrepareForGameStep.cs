using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class PrepareForGameStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var footballContext = context.Environment.FootballContext;

            // Check if any non-complete seasons exist
            if (!footballContext.SeasonRecords.Any(sr => !sr.SeasonComplete))
            {
                Log.Information("PrepareForGameStep: No incomplete season found, initializing next season.")
                return context.WithNextState(SystemState.InitializeNextSeason);
            }

            if (footballContext.SeasonRecords.Count(sr => !sr.SeasonComplete) > 1)
            {
                throw new InvalidOperationException("More than one incomplete season exists in the database.");
            }

            // Check if a complete season exists but has no summary
            var currentSeason = footballContext.SeasonRecords
                .OrderByDescending(sr => sr.Year)
                .First(sr => !sr.SeasonComplete);
            var existingSummary = footballContext.Summaries
                .Where(s => s.SeasonRecordID == currentSeason.SeasonRecordID
                    && s.GameRecordID == null)
                .SingleOrDefault();
            if (existingSummary == null)
            {
                Log.Information("PrepareForGameStep: Complete season found with no summary, writing summary for season {SeasonYear}.",
                    currentSeason.Year);
                return context.WithNextState(SystemState.WriteSummaryForSeason);
            }

            // Check if any non-complete games exist
            var seasonGames = footballContext.GameRecords
                .Include(g => g.TeamDriveRecords)
                .Where(g => g.SeasonRecordID == currentSeason.SeasonRecordID)
                .ToList();
            if (seasonGames.Any(g => !g.GameComplete))
            {
                // Check if any partially complete games exist
                var partialGame = seasonGames
                    .Where(g => !g.GameComplete)
                    .SingleOrDefault(g => g.TeamGameRecords.Count > 0);
                if (partialGame != null)
                {
                    Log.Information("PrepareForGameStep: Found partially complete game (GameID: {GameID}) for season {SeasonYear}, resuming game.",
                        partialGame.GameID,
                        currentSeason.Year);
                    return context.WithNextState(SystemState.ResumePartialGame);
                }

                Log.Information("PrepareForGameStep: Found next game for season {SeasonYear}, loading game.",
                    currentSeason.Year);
                return context.WithNextState(SystemState.LoadGame);
            }

            // Check if all games are complete but no playoff games exist
            var postseasonGames = seasonGames
                .Where(g => g.GameType == GameType.Postseason)
                .ToList();
            if (seasonGames.All(g => g.GameComplete))
            {
                if (postseasonGames.Count == 0)
                {
                    Log.Information("PrepareForGameStep: All regular season games complete for season {SeasonYear}, initializing Wild Card round.",
                        currentSeason.Year);
                    return context.WithNextState(SystemState.InitializeWildCardRound);
                }
                
                if (postseasonGames.Count == 8)
                {
                    Log.Information("PrepareForGameStep: All Wild Card games complete for season {SeasonYear}, initializing Divisional round.",
                        currentSeason.Year);
                    return context.WithNextState(SystemState.InitializeDivisionalRound);
                }

                if (postseasonGames.Count == 12)
                {
                    Log.Information("PrepareForGameStep: All Divisional round games complete for season {SeasonYear}, initializing Conference Championship round.",
                        currentSeason.Year);
                    return context.WithNextState(SystemState.InitializeConferenceChampionshipRound);
                }

                if (postseasonGames.Count == 14)
                {
                    Log.Information("PrepareForGameStep: All Conference Championship games complete for season {SeasonYear}, initializing Super Bowl.",
                        currentSeason.Year);
                    return context.WithNextState(SystemState.InitializeSuperBowl);
                }

                throw new InvalidOperationException($"Unexpected number of postseason games, {postseasonGames.Count}, found for {currentSeason.Year}.");
            }

            throw new InvalidOperationException("Reached an unexpected, presumed unreachable state in PrepareForGameStep.");
        }
    }
}
