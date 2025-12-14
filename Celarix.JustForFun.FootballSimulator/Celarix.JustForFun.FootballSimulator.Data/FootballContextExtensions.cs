using Celarix.JustForFun.FootballSimulator.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data
{
    public static class FootballContextExtensions
    {
        extension(FootballContext context)
        {
            public int GetCurrentSeasonId()
            {
                return context.SeasonRecords
                    .OrderBy(sr => sr.Year)
                    .First(sr => !sr.SeasonComplete)
                    .SeasonRecordID;
            }

            public GameRecord GetCurrentGameRecord(int seasonRecordId)
            {
                return context.GameRecords
                    .Include(gr => gr.HomeTeam)
                    .Include(gr => gr.AwayTeam)
                    .Include(gr => gr.Stadium)
                    .Include(gr => gr.QuarterBoxScores)
                    .Include(gr => gr.TeamGameRecords)
                    .Include(gr => gr.TeamDriveRecords)
                        .ThenInclude(tdr => tdr.Team)
                    .Where(gr => gr.SeasonRecordID == seasonRecordId)
                    .OrderBy(gr => gr.WeekNumber)
                    .OrderBy(gr => gr.KickoffTime)
                    .OrderBy(gr => gr.GameID)
                    .First(gr => !gr.GameComplete);
            }

            public IReadOnlyDictionary<string, PhysicsParam> GetAllPhysicsParams()
            {
                return context.PhysicsParams
                    .AsNoTracking()
                    .ToDictionary(pp => pp.Name, pp => pp);
            }

            public IReadOnlyList<Player> GetActivePlayersForTeam(int teamId)
            {
                return context.Players
                .Include(p => p.RosterPositions.Where(rp => rp.TeamID == teamId && rp.CurrentPlayer))
                .Where(p => p.RosterPositions.Any(rp => rp.TeamID == teamId && rp.CurrentPlayer))
                .ToList();
            }
        }

        extension(Player player)
        {
            public Player CurrentRosterPositionOnly()
            {
                player.RosterPositions = [.. player.RosterPositions.Where(rp => rp.CurrentPlayer)];
                if (player.RosterPositions.Count > 1)
                {
                    throw new InvalidOperationException($"Player {player.PlayerID} has multiple current roster positions.");
                }
                return player;
            }
        }
    }
}
