using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;

namespace Celarix.JustForFun.FootballSimulator;

public sealed class MainLoop
{
    private readonly FootballContext context;
    
    public string GameStatusMessage { get; private set; }

    public MainLoop() => context = new FootballContext();

    public void RunNextAction()
    {
        if (DatabaseRequiredInitialization())
        {
            GameStatusMessage = "Added basic team info to database.";

            return;
        }
        
        if (GetCurrentSeasonYear() == null)
        {
            var newSeasonYear = (GetLastSeasonYear() ?? 2014) + 1;

            var seasonRecord = new SeasonRecord
            {
                Year = newSeasonYear
            };
            context.SeasonRecords.Add(seasonRecord);
            context.SaveChanges();

            var seasonSchedule = ScheduleGenerator.GetPreseasonAndRegularSeasonGamesForSeason(newSeasonYear,
                context.Teams.ToList(), )
        }
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

    private Dictionary<string, int> GetTeamFinishingPositionsForSeason(int seasonYear)
    {
        var teamFinishingPositions = new Dictionary<int, string>();
        var teamsByDivision = context.Teams
            .ToList()
            .GroupBy(t => new
            {
                t.Conference, t.Division
            });

        foreach (var division in teamsByDivision)
        {
            
        }
    }
}