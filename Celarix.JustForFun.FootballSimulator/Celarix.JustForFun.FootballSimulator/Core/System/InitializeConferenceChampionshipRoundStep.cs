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
            var repository = context.Environment.FootballRepository;
            var currentSeason = repository.GetMostRecentSeason()
                ?? throw new InvalidOperationException("No current season found.");
            var divisionalRoundGames = repository.GetPlayoffGamesForSeason(currentSeason.SeasonRecordID,
                PlayoffRound.Divisional);
            var teamPlayoffSeeds = repository.GetPlayoffSeedsForSeason(currentSeason.SeasonRecordID);

            if (divisionalRoundGames.Count != 4)
            {
                throw new InvalidOperationException("There should be exactly 4 divisional round games to proceed to the conference championships.");
            }

            if (divisionalRoundGames.Any(g => g.AwayTeam == null || g.HomeTeam == null))
            {
                throw new InvalidOperationException("Teams not loaded from database.");
            }

            if (!divisionalRoundGames.All(g => g.GameComplete))
            {
                throw new InvalidOperationException("All divisional round games must be complete to proceed to the conference championships.");
            }

            if (divisionalRoundGames.Any(g => g.WinningTeam == WinningTeam.Tie))
            {
                throw new InvalidOperationException("No divisional round games can end in a tie to proceed to the conference championships.");
            }

            // We check above that all teams are non-null, so these warnings are safe to suppress
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8604
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
                .AtMidnight()
                .AddDays(8);

            var conferenceChampionshipKickoffTimes = new List<DateTimeOffset>
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

            repository.AddGameRecords(conferenceChampionships);
            repository.SaveChanges();

            Log.Information("InitializeConferenceChampionshipRoundStep: Created Conference Championships.");
            return context.WithNextState(SystemState.LoadGame);
#pragma warning restore CS8600
#pragma warning restore CS8602
#pragma warning restore CS8604
        }

        internal static GameRecord MakeConferenceChampionship(Team home, Team away,
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
