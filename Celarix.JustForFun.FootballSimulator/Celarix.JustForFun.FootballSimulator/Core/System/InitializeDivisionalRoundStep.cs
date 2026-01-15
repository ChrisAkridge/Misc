using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class InitializeDivisionalRoundStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var currentSeason = repository.GetMostRecentSeason()
                ?? throw new InvalidOperationException("No season records found in database.");
            var wildCardGames = repository.GetPlayoffGamesForSeason(currentSeason.SeasonRecordID, PlayoffRound.WildCard);
            var teamPlayoffSeeds = repository.GetPlayoffSeedsForSeason(currentSeason.SeasonRecordID);

            if (wildCardGames.Count != 8)
            {
                throw new InvalidOperationException("There should be exactly 8 wild card games to proceed to the divisional round.");
            }

            if (!wildCardGames.All(g => g.GameComplete))
            {
                throw new InvalidOperationException("All wild card games must be complete to proceed to the divisional round.");
            }

            if (wildCardGames.Any(g => g.WinningTeam == WinningTeam.Tie))
            {
                throw new InvalidOperationException("No wild card games can end in a tie to proceed to the divisional round.");
            }

            var wildCardWinners = wildCardGames
                .Select(g =>
                {
                    var winningTeam = g.WinningTeam;
                    Team winningTeamRecord = winningTeam == WinningTeam.Home ? g.HomeTeam : g.AwayTeam;
                    return new
                    {
                        Team = winningTeamRecord,
                        teamPlayoffSeeds
                            .Single(tps => tps.TeamID == winningTeamRecord.TeamID)
                            .Seed
                    };
                })
                .ToList();
            var afcWinners = wildCardWinners
                .Where(wcw => wcw.Team.Conference == Conference.AFC)
                .OrderBy(wcw => wcw.Seed)
                .ToList();
            var nfcWinners = wildCardWinners
                .Where(wcw => wcw.Team.Conference == Conference.NFC)
                .OrderBy(wcw => wcw.Seed)
                .ToList();

            var divisionalRoundSaturday = wildCardGames
                .OrderBy(g => g.KickoffTime)
                .First()
                .KickoffTime
                .Date
                .AddDays(7);
            var divisionalRoundSunday = divisionalRoundSaturday.AddDays(1);

            var divisionalRoundKickoffTimes = new List<DateTime>
            {
                divisionalRoundSaturday.AddHours(16).AddMinutes(25),
                divisionalRoundSaturday.AddHours(20).AddMinutes(20),
                divisionalRoundSunday.AddHours(16).AddMinutes(25),
                divisionalRoundSunday.AddHours(20).AddMinutes(20)
            };

            var divisionalRoundGames = new List<GameRecord>
            {
                MakeDivisionalRoundGame(afcWinners[0].Team, afcWinners[3].Team,
                    divisionalRoundKickoffTimes[0], currentSeason.SeasonRecordID),
                MakeDivisionalRoundGame(afcWinners[1].Team, afcWinners[2].Team,
                    divisionalRoundKickoffTimes[1], currentSeason.SeasonRecordID),
                MakeDivisionalRoundGame(nfcWinners[0].Team, nfcWinners[3].Team,
                    divisionalRoundKickoffTimes[2], currentSeason.SeasonRecordID),
                MakeDivisionalRoundGame(nfcWinners[1].Team, nfcWinners[2].Team,
                    divisionalRoundKickoffTimes[3], currentSeason.SeasonRecordID)
            };

            repository.AddGameRecords(divisionalRoundGames);
            repository.SaveChanges();

            Log.Information("InitializeDivisionalRoundStep: Divisional Round games initialized.");
            return context.WithNextState(SystemState.LoadGame);
        }

        internal static GameRecord MakeDivisionalRoundGame(Team away, Team home,
            DateTimeOffset kickoffTime, int seasonRecordID)
        {
            Log.Information($"InitializeDivisionalRoundStep: " +
                $"Creating Divisional Round game between {away.TeamName} " +
                $"at {home.TeamName} at {kickoffTime}.");
            return new GameRecord
            {
                SeasonRecordID = seasonRecordID,
                GameType = GameType.Postseason,
                HomeTeamID = home.TeamID,
                AwayTeamID = away.TeamID,
                KickoffTime = kickoffTime,
                StadiumID = home.HomeStadiumID,
                WeekNumber = 19,
                GameComplete = false,
                HomeTeamStrengthsAtKickoffJSON = home.GetStrengthJson(),
                AwayTeamStrengthsAtKickoffJSON = away.GetStrengthJson()
            };
        }
    }
}
