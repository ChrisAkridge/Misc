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
                return [.. context.Players
                .Include(p => p.RosterPositions!.Where(rp => rp.TeamID == teamId && rp.CurrentPlayer))
                .Where(p => p.RosterPositions!.Any(rp => rp.TeamID == teamId && rp.CurrentPlayer))];
            }

            public Stadium GetHomeStadiumForTeam(int teamID)
            {
                return context.Teams
                    .Include(t => t.HomeStadium)
                    .First(t => t.TeamID == teamID)
                    .HomeStadium!;
            }

            public void HealAllPlayers()
            {
                // Remove 1 week from GamesUntilReturnFromInjury for all players who have it set
                var injuredPlayers = context.PlayerRosterPositions
                    .Where(prp => prp.GamesUntilReturnFromInjury.HasValue)
                    .ToList();

                foreach (var player in injuredPlayers)
                {
                    player.GamesUntilReturnFromInjury--;
                }

                context.SaveChanges();
            }
        }

        extension(Player player)
        {
            public Player CurrentRosterPositionOnly()
            {
                if (player.RosterPositions == null)
                {
                    throw new InvalidOperationException($"Player {player.PlayerID} has no loaded roster positions from the database.");
                }
                player.RosterPositions = [.. player.RosterPositions.Where(rp => rp.CurrentPlayer)];
                if (player.RosterPositions.Count > 1)
                {
                    throw new InvalidOperationException($"Player {player.PlayerID} has multiple current roster positions.");
                }
                return player;
            }
        }

        extension(GameRecord game)
        {
            public int GetScoreForTeam(GameTeam team)
            {
                var quarterBoxScoresForTeam = game.QuarterBoxScores
                    .Where(qbs => qbs.Team == team);
                return quarterBoxScoresForTeam.Sum(qbs => qbs.Score);
            }

            public GameTeam? GetWinningTeam()
            {
                var homeScore = GetScoreForTeam(game, GameTeam.Home);
                var awayScore = GetScoreForTeam(game, GameTeam.Away);
                if (homeScore > awayScore)
                {
                    return GameTeam.Home;
                }
                else if (awayScore > homeScore)
                {
                    return GameTeam.Away;
                }
                else
                {
                    return null;
                }
            }
        }

        extension(Team team)
        {
            public string GetStrengthJson()
            {
                var strengths = new
                {
                    team.RunningOffenseStrength,
                    team.RunningDefenseStrength,
                    team.PassingOffenseStrength,
                    team.PassingDefenseStrength,
                    team.OffensiveLineStrength,
                    team.DefensiveLineStrength,
                    team.KickingStrength,
                    team.FieldGoalStrength,
                    team.KickReturnStrength,
                    team.KickDefenseStrength,
                    team.ClockManagementStrength
                };

                return System.Text.Json.JsonSerializer.Serialize(strengths);
            }
        }
    }
}
