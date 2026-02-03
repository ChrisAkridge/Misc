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
    internal static class InitializeSuperBowlStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var currentSeason = repository.GetMostRecentSeason()
                ?? throw new InvalidOperationException("No current season found.");
            var conferenceChampionships = repository.GetPlayoffGamesForSeason(currentSeason.SeasonRecordID, PlayoffRound.ConferenceChampionship);
            var teamPlayoffSeeds = repository.GetPlayoffSeedsForSeason(currentSeason.SeasonRecordID);

            if (conferenceChampionships.Count != 2)
            {
                throw new InvalidOperationException("There should be exactly 2 conference championships to proceed to the Super Bowl.");
            }

            if (!conferenceChampionships.All(g => g.GameComplete))
            {
                throw new InvalidOperationException("All conference championships must be complete to proceed to the Super Bowl.");
            }

            if (conferenceChampionships.Any(g => g.WinningTeam == WinningTeam.Tie))
            {
                throw new InvalidOperationException("No conference championships can end in a tie to proceed to the Super Bowl.");
            }

            var conferenceChampionshipWinners = conferenceChampionships
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
            var afcWinner = conferenceChampionshipWinners
                .Where(wcw => wcw.Team.Conference == Conference.AFC)
                .OrderBy(wcw => wcw.Seed)
                .Single();
            var nfcWinner = conferenceChampionshipWinners
                .Where(wcw => wcw.Team.Conference == Conference.NFC)
                .OrderBy(wcw => wcw.Seed)
                .Single();

            var superBowlKickoffTime = conferenceChampionships
                .OrderBy(g => g.KickoffTime)
                .First()
                .KickoffTime
                .AtMidnight()
                .AddDays(14)
                .AddHours(18)
                .AddMinutes(5);

            var home = currentSeason.Year % 2 == 0
                ? afcWinner.Team : nfcWinner.Team;
            var away = currentSeason.Year % 2 == 0
                ? nfcWinner.Team : afcWinner.Team;

            Log.Information($"InitializeSuperBowlStep: " +
                $"Creating Super Bowl between {away.TeamName} " +
                $"at {home.TeamName} at {superBowlKickoffTime}.");
            var superBowl = new GameRecord
            {
                SeasonRecordID = currentSeason.SeasonRecordID,
                GameType = GameType.Postseason,
                HomeTeamID = home.TeamID,
                AwayTeamID = away.TeamID,
                KickoffTime = superBowlKickoffTime,
                StadiumID = home.HomeStadiumID,
                WeekNumber = 22,
                GameComplete = false,
                HomeTeamStrengthsAtKickoffJSON = home.GetStrengthJson(),
                AwayTeamStrengthsAtKickoffJSON = away.GetStrengthJson()
            };

            repository.AddGameRecord(superBowl);
            repository.SaveChanges();

            Log.Information("InitializeSuperBowlStep: Super Bowl initialized.");
            return context.WithNextState(SystemState.LoadGame);
        }
    }
}
