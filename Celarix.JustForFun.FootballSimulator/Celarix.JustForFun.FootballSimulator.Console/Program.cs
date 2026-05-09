using Celarix.JustForFun.FootballSimulator;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Core;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output;
using Celarix.JustForFun.FootballSimulator.Random;
using Celarix.JustForFun.FootballSimulator.Scheduling;
using Serilog;

ConfigureFileLogging();

var eventBus = new EventBus();
var basicConsoleListener = new BasicConsoleListener();
eventBus.Subscribe(basicConsoleListener);
var systemLoop = new SystemLoop(eventBus);

while (true)
{
    systemLoop.MoveNext();
    Console.ReadKey(intercept: true);
}

static void ConfigureConsoleLogging()
{
	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Debug()
		.WriteTo.Console()
		.CreateLogger();
}

static void ConfigureFileLogging()
{
    // Probably should add this path to the database at some point, but for now, we'll just hardcode it.
    var logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Football Simulator", "log-.txt");
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
        .CreateLogger();
}