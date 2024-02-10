using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Gameplay;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Tiebreaking;
using Microsoft.EntityFrameworkCore;

namespace Celarix.JustForFun.FootballSimulator;

public sealed class MainLoop
{
    private readonly FootballContext context;
    private FootballGame? currentGame;
    
    public string StatusMessage { get; private set; }

    public MainLoop() => context = new FootballContext();

    public void RunNextAction()
    {
        if (DatabaseRequiredInitialization())
        {
            StatusMessage = "Added basic team info to database.";

            return;
        }

        if (GetCurrentSeasonYear() == null)
        {
            AddNextSeasonToDatabase();

            return;
        }

        if (currentGame != null)
        {
            currentGame.RunNextAction();
            StatusMessage = currentGame.StatusMessage;

            if (currentGame.GameOver)
            {
                currentGame.MarkGameRecordComplete();
                currentGame = null;
            }
        }
        else
        {
            var nextUnplayedGame = GetNextUnplayedGame();

            if (nextUnplayedGame != null) { currentGame = new FootballGame(context, nextUnplayedGame); }
        }
    }

    private void AddNextSeasonToDatabase()
    {
        var teams = context.Teams.ToArray();
        var previousSeasonYear = GetLastSeasonYear();
        var newSeasonYear = (previousSeasonYear ?? 2014) + 1;

        var seasonRecord = new SeasonRecord
        {
            Year = newSeasonYear
        };
        var seasonRecordEntity = context.SeasonRecords.Add(seasonRecord);
        context.SaveChanges();

        var previousSeasonGames = previousSeasonYear != null
            ? context.GameRecords
                .Include(g => g.SeasonRecord)
                .Where(g => g.SeasonRecord.Year == previousSeasonYear)
            : Enumerable.Empty<GameRecord>();
        var divisionTiebreaker = new DivisionTiebreaker(context.Teams.ToList(), previousSeasonGames);
        Dictionary<string, int>? previousSeasonTeamPositions = null;

        if (previousSeasonYear != null)
        {
            var divisionsInStandingOrder = divisionTiebreaker.GetTeamsInDivisionStandingsOrder();
            previousSeasonTeamPositions = new Dictionary<string, int>();

            foreach (var divisionKVP in divisionsInStandingOrder)
            {
                for (int i = 0; i < 4; i++) { previousSeasonTeamPositions.Add(divisionKVP.Value[i].TeamName, i + 1); }
            }
        }

        //var seasonSchedule = ScheduleGenerator.GetPreseasonAndRegularSeasonGamesForSeason(newSeasonYear,
        //    context.Teams.ToList(), previousSeasonTeamPositions);

        //foreach (var gameRecord in seasonSchedule)
        //{
        //    gameRecord.SeasonRecordID = seasonRecordEntity.Entity.SeasonRecordID;
        //    gameRecord.StadiumID = teams.First(t => t.TeamName == gameRecord.HomeTeam.TeamName).HomeStadiumID;
        //}

        //context.GameRecords.AddRange(seasonSchedule);
        //context.SaveChanges();

        StatusMessage = $"Created schedule for the {newSeasonYear} season!";
    }

    private bool DatabaseRequiredInitialization()
    {
        var settings = context.SimulatorSettings.SingleOrDefault();

        if (settings?.SeedDataInitialized != true)
        {
            if (settings == null)
            {
                context.SimulatorSettings.Add(new SimulatorSettings
                {
                    SeedDataInitialized = true
                });
                context.SaveChanges();
            }
            else { settings.SeedDataInitialized = true; }

            var teamsWithStadiums = SeedData.TeamSeedData();

            foreach (var team in teamsWithStadiums) { context.Teams.Add(team); }

            context.SaveChanges();

            return true;
        }

        return false;
    }

    private int? GetCurrentSeasonYear()
    {
        var mostRecentIncompleteSeason = context.SeasonRecords
            .SingleOrDefault(s => !s.SeasonComplete);

        return mostRecentIncompleteSeason?.Year;
    }

    private int? GetLastSeasonYear()
    {
        return context.SeasonRecords
            .Where(s => s.SeasonComplete)
            .OrderByDescending(s => s.Year)
            .FirstOrDefault()
            ?.Year;
    }

    private GameRecord? GetNextUnplayedGame()
    {
        return context.GameRecords
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .Where(g => !g.GameComplete)
            .ToArray()
            .MinBy(g => g.KickoffTime);
    }
}