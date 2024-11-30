using Celarix.JustForFun.FootballSimulator;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;

//var mainLoop = new MainLoop();

//while (true)
//{
//    mainLoop.RunNextAction();
//    Console.WriteLine(mainLoop.StatusMessage);
//    Thread.Sleep(1000);
//}

ConfigureLogging();

var context = new FootballContext();
context.Database.EnsureCreated();
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

var teams = context.Teams.ToList();
var dataTeams = teams.ToDictionary(t => new BasicTeamInfo(t.TeamName, t.Conference, t.Division), t => t);
var scheduleGenerator = new ScheduleGenerator3(dataTeams.Keys.ToArray());
var schedule = scheduleGenerator.GenerateScheduleForYear(2014, dataTeams, null, null);

return;

void ConfigureLogging()
{
	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.WriteTo.Console()
		.CreateLogger();
}