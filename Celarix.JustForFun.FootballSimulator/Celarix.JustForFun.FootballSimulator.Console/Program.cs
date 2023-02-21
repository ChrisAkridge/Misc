using Celarix.JustForFun.FootballSimulator.Data.Models;

var footballContext = new FootballContext();
InitializeDatabase(footballContext);
var teams = footballContext.Teams.ToList();

var schedule =
    Celarix.JustForFun.FootballSimulator.ScheduleGenerator
        .GetPreseasonAndRegularSeasonGamesForSeason(2023, teams, null);

Console.WriteLine("");

// should probably move this to Celarix.JustForFun.FootballSimulator
void InitializeDatabase(FootballContext context)
{
    // Okay, this is probably not the best place to do this.
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
        else
        {
            settings.SeedDataInitialized = true;
        }

        var teamsWithStadiums = SeedData.TeamSeedData();

        foreach (var team in teamsWithStadiums) { context.Teams.Add(team); }

        context.SaveChanges();
    }
}