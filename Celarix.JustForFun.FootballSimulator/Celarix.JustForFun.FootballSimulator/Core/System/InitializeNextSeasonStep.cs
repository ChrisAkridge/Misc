using Celarix.JustForFun.FootballSimulator.Data;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
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
            var footballContext = context.Environment.FootballContext;
            var nextSeasonYear = 2014;

            var teams = footballContext.Teams.ToList();
            var dataTeams = teams.ToDictionary(t => new BasicTeamInfo(t), t => t);
            BasicTeamInfo[] teamList = [.. dataTeams.Keys];
            var scheduleGenerator = context.Environment.ScheduleGenerator
                ??= new ScheduleGenerator3(teamList, context.Environment.RandomFactory);
            var random = context.Environment.RandomFactory.Create();

            var mostRecentSeason = footballContext.SeasonRecords.OrderByDescending(sr => sr.Year)
                .FirstOrDefault();
            Team? previousSuperBowlWinner = null;
            Dictionary<BasicTeamInfo, int>? divisionStandings = null;
            if (mostRecentSeason != null)
            {
                nextSeasonYear = mostRecentSeason.Year + 1;

                var regularSeasonGames = footballContext.GameRecords
                    .Where(g => g.SeasonRecordID == mostRecentSeason.SeasonRecordID
                        && g.GameType == GameType.RegularSeason)
                    .ToList();

                var previousSuperBowl = footballContext.GameRecords
                .Include(g => g.HomeTeam)
                .Include(g => g.AwayTeam)
                .Where(g => g.SeasonRecordID == mostRecentSeason.SeasonRecordID
                    && g.GameType == GameType.Postseason
                    && g.HomeTeam.Conference != g.AwayTeam.Conference)
                .SingleOrDefault();
                var winner = previousSuperBowl?.GetWinningTeam();
                previousSuperBowlWinner = winner == GameTeam.Home
                    ? previousSuperBowl?.HomeTeam
                    : winner == GameTeam.Away
                        ? previousSuperBowl?.AwayTeam
                        : null;

                context.Environment.DivisionTiebreaker ??= new Tiebreaking.DivisionTiebreaker(teams,
                    regularSeasonGames,
                    random);
                divisionStandings = context.Environment.DivisionTiebreaker.GetTeamDivisionRankings();
            }

            if (previousSuperBowlWinner == null)
            {
                previousSuperBowlWinner = teams[random.Next(teams.Count)];
                divisionStandings = Helpers.GetDefaultPreviousSeasonDivisionRankings(teamList);
            }

            var schedule = scheduleGenerator.GenerateScheduleForYear(nextSeasonYear,
                dataTeams,
                divisionStandings,
                new BasicTeamInfo(previousSuperBowlWinner),
                out _);
            footballContext.GameRecords.AddRange(schedule);
            footballContext.SeasonRecords.Add(new SeasonRecord
            {
                Year = nextSeasonYear
            });
            footballContext.SaveChanges();

            Log.Information("InitializeNextSeasonStep: Initialized next season ({NextSeasonYear}) schedule.",
                nextSeasonYear);
            return context.WithNextState(SystemState.LoadGame);
        }
    }
}
