using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Celarix.JustForFun.FootballSimulator.Tiebreaking;
using Microsoft.EntityFrameworkCore;

namespace Celarix.JustForFun.FootballSimulator;

public sealed class MainLoop
{
    private readonly FootballContext context;
    
    public string GameStatusMessage { get; private set; }

    public MainLoop() => context = new FootballContext();

    public void RunNextAction()
    {
        //if (DatabaseRequiredInitialization())
        //{
        //    GameStatusMessage = "Added basic team info to database.";

        //    return;
        //}
        
        //if (GetCurrentSeasonYear() == null)
        //{
        //var previousSeasonYear = GetLastSeasonYear();
        //var newSeasonYear = (previousSeasonYear ?? 2014) + 1;
        var newSeasonYear = 2016;

        //var seasonRecord = new SeasonRecord
        //{
        //    Year = newSeasonYear
        //};
        //var seasonRecordEntity = context.SeasonRecords.Add(seasonRecord);
        //context.SaveChanges();

        var previousSeasonGames = /* previousSeasonYear != null
            ? context.GameRecords
                .Include(g => g.SeasonRecord)
                .Where(g => g.SeasonRecord.Year == previousSeasonYear)
            : */ Enumerable.Empty<GameRecord>();
        var divisionTiebreaker = new DivisionTiebreaker(context.Teams.ToList(), previousSeasonGames);
        Dictionary<string, int>? previousSeasonTeamPositions = null;
        var seasonRecord = context.SeasonRecords.First(s => s.Year == 2015);

        //if (previousSeasonYear != null)
        //{
        //    var divisionsInStandingOrder = divisionTiebreaker.GetTeamsInDivisionStandingsOrder();
        //    previousSeasonTeamPositions = new Dictionary<string, int>();

        //    foreach (var divisionKVP in divisionsInStandingOrder)
        //    {
        //        for (int i = 0; i < 4; i++)
        //        {
        //            previousSeasonTeamPositions.Add(divisionKVP.Value[i].TeamName, i + 1);
        //        }
        //    }
        //}

        var seasonSchedule = ScheduleGenerator.GetPreseasonAndRegularSeasonGamesForSeason(newSeasonYear,
            context.Teams.ToList(), previousSeasonTeamPositions);

        foreach (var gameRecord in seasonSchedule)
        {
            gameRecord.SeasonRecordID = seasonRecord.SeasonRecordID; /* seasonRecordEntity.Entity.SeasonRecordID */;
        }
            
        context.GameRecords.AddRange(seasonSchedule);
        GameStatusMessage = $"Created schedule for the {newSeasonYear} season!";

        return;
        //}
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
        var mostRecentUnplayedGames = context.SeasonRecords
            .SingleOrDefault(s => !s.SeasonComplete);

        return mostRecentUnplayedGames?.Year;
    }

    private int? GetLastSeasonYear()
    {
        return context.SeasonRecords
            .Where(s => s.SeasonComplete)
            .OrderByDescending(s => s.Year)
            .FirstOrDefault()
            ?.Year;
    }
}