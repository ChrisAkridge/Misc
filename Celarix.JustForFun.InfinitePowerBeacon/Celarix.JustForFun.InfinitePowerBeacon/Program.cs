using Celarix.JustForFun.InfinitePowerBeacon.Simulation;

var simulation = new Simulation();
var result = simulation.RunSimulation();

// Print the items in every step to the console,
// then the final costs.
for (var i = 0; i < result.Steps.Count; i++)
{
	var step = result.Steps[i];
	Console.WriteLine($"Step {i + 1}:");

	foreach (var item in step.AllItems) { Console.WriteLine($"  {item}"); }
}

Console.WriteLine($"Total Smelting Cost: {result.TotalSmeltingCost}");
Console.WriteLine($"Total Compression Cost: {result.TotalCompressionCost}");
Console.WriteLine($"Total Hypercompression Cost: {result.TotalHypercompressionCost}");
Console.WriteLine($"Total Mining Time: {SecondsToHHMMSS(result.TotalMiningTime)}");
Console.WriteLine($"Total Killing Cost: {result.TotalKillingCost}");

string SecondsToHHMMSS(decimal seconds)
{
	var ts = TimeSpan.FromSeconds((double)seconds);

	return ts.ToString();
}