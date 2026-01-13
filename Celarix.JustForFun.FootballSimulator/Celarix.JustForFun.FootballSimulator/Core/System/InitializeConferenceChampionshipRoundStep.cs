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
    internal static class InitializeConferenceChampionshipRoundStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var footballContext = context.Environment.FootballContext;
            var currentSeason = footballContext.SeasonRecords
                .OrderByDescending(sr => sr.Year)
                .First();
            var divisionalRoundGames = footballContext.GameRecords
                .Include(g => g.AwayTeam)
                .Include(g => g.HomeTeam)
                .Where(gr => gr.SeasonRecordID == currentSeason.SeasonRecordID
                    && gr.GameType == GameType.Postseason
                    && gr.WeekNumber == 19)
                .ToList();
            var teamPlayoffSeeds = footballContext.TeamPlayoffSeeds
                .Where(tps => tps.SeasonRecordID == currentSeason.SeasonRecordID)
                .ToList();

            if (divisionalRoundGames.Count != 4)
            {
                throw new InvalidOperationException("There should be exactly 4 divisional round games to proceed to the conference championships.");
            }

            if (!divisionalRoundGames.All(g => g.GameComplete))
            {
                throw new InvalidOperationException("All divisional round games must be complete to proceed to the conference championships.");
            }

            if (divisionalRoundGames.Any(g => g.WinningTeam == WinningTeam.Tie))
            {
                throw new InvalidOperationException("No divisional round games can end in a tie to proceed to the conference championships.");
            }

            var divisionalRoundWinners = divisionalRoundGames
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
            var afcWinners = divisionalRoundWinners
                .Where(wcw => wcw.Team.Conference == Conference.AFC)
                .OrderBy(wcw => wcw.Seed)
                .ToList();
            var nfcWinners = divisionalRoundWinners
                .Where(wcw => wcw.Team.Conference == Conference.NFC)
                .OrderBy(wcw => wcw.Seed)
                .ToList();

            var conferenceChampionshipSunday = divisionalRoundGames
                .OrderBy(g => g.KickoffTime)
                .First()
                .KickoffTime
                .Date
                .AddDays(8);

            var conferenceChampionshipKickoffTimes = new List<DateTime>
            {
                conferenceChampionshipSunday.AddHours(16).AddMinutes(25),
                conferenceChampionshipSunday.AddHours(20).AddMinutes(20)
            };

            var conferenceChampionships = new List<GameRecord>
            {
                MakeConferenceChampionship(afcWinners[0].Team, afcWinners[1].Team,
                    conferenceChampionshipKickoffTimes[0], currentSeason.SeasonRecordID),
                MakeConferenceChampionship(nfcWinners[0].Team, nfcWinners[1].Team,
                    conferenceChampionshipKickoffTimes[1], currentSeason.SeasonRecordID)
            };

            footballContext.GameRecords.AddRange(conferenceChampionships);
            footballContext.SaveChanges();

            Log.Information("InitializeConferenceChampionshipRoundStep: Created Conference Championships.");
            return context.WithNextState(SystemState.LoadGame);
        }

        internal static GameRecord MakeConferenceChampionship(Team away, Team home,
            DateTimeOffset kickoffTime, int seasonRecordID)
        {
            Log.Information($"InitializeConferenceChampionshipRoundStep: " +
                $"Creating Conference Championship game between {away.TeamName} " +
                $"at {home.TeamName} at {kickoffTime}.");
            return new GameRecord
            {
                SeasonRecordID = seasonRecordID,
                GameType = GameType.Postseason,
                HomeTeamID = home.TeamID,
                AwayTeamID = away.TeamID,
                KickoffTime = kickoffTime,
                StadiumID = home.HomeStadiumID,
                WeekNumber = 20,
                GameComplete = false,
                HomeTeamStrengthsAtKickoffJSON = home.GetStrengthJson(),
                AwayTeamStrengthsAtKickoffJSON = away.GetStrengthJson()
            };
        }
    }
}
