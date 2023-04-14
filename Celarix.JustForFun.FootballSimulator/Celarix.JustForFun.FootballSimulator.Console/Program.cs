using Celarix.JustForFun.FootballSimulator;

var mainLoop = new MainLoop();

while (true)
{
    mainLoop.RunNextAction();
    Console.WriteLine(mainLoop.StatusMessage);
    Thread.Sleep(1000);
}