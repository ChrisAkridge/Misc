using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Standings;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.System
{
    internal static class InitializeNextSeasonStep
    {
        public static SystemContext Run(SystemContext context)
        {
            var repository = context.Environment.FootballRepository;
            var nextSeasonYear = 2014;

            var teams = repository.GetTeams();
            var basicTeamInfos = teams.ToDictionary(t => new BasicTeamInfo(t), t => t);
            BasicTeamInfo[] teamList = [.. basicTeamInfos.Keys];
            var scheduleGenerator = context.Environment.ScheduleGenerator
                ??= new ScheduleGenerator3(teamList, context.Environment.RandomFactory);
            var random = context.Environment.RandomFactory.Create();

            var mostRecentSeason = repository.GetMostRecentSeason();
            Team? previousSuperBowlWinner = null;
            Dictionary<BasicTeamInfo, int>? divisionStandings = null;
            if (mostRecentSeason != null)
            {
                nextSeasonYear = mostRecentSeason.Year + 1;

                var regularSeasonGames = repository.GetGameRecordsForSeasonByGameType(mostRecentSeason.SeasonRecordID,
                    GameType.RegularSeason);

                var previousSuperBowl = repository.GetPlayoffGamesForSeason(mostRecentSeason.SeasonRecordID,
                    PlayoffRound.SuperBowl).FirstOrDefault();
                var winner = previousSuperBowl?.GetWinningTeam();
                previousSuperBowlWinner = winner == GameTeam.Home
                    ? previousSuperBowl?.HomeTeam
                    : winner == GameTeam.Away
                        ? previousSuperBowl?.AwayTeam
                        : null;

                context.Environment.TeamRanker ??= new TeamRanker(regularSeasonGames, teams);
                divisionStandings = new(teams
                    .GroupBy(t => new
                    {
                        t.Conference,
                        t.Division
                    })
                    .Select(d => context.Environment.TeamRanker.RankTeamsAndBreakCoinFlipTies(d.Select(t => new BasicTeamInfo(t)), random))
                    .SelectMany(tvr => tvr.ToDictionary(tvr => tvr.BasicTeamInfo, tvr => tvr.Ranking)));
            }

            if (previousSuperBowlWinner == null)
            {
                previousSuperBowlWinner = teams[random.Next(teams.Count)];
                divisionStandings = Helpers.GetDefaultPreviousSeasonDivisionRankings(teamList);
            }

            var schedule = scheduleGenerator.GenerateScheduleForYear(nextSeasonYear,
                basicTeamInfos,
                divisionStandings,
                new BasicTeamInfo(previousSuperBowlWinner),
                out _);
            repository.AddGameRecords(schedule);
            repository.AddSeasonRecord(new SeasonRecord
            {
                Year = nextSeasonYear
            });
            repository.SaveChanges();

            Log.Information("InitializeNextSeasonStep: Initialized next season ({NextSeasonYear}) schedule.",
                nextSeasonYear);
            return context.WithNextState(SystemState.LoadGame);
        }
    }
}
