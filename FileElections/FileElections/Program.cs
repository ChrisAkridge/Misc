using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileElections
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.SetBufferSize(80, 1000);

			Console.WriteLine("File Election Simulator:");
			Console.WriteLine();
			Console.WriteLine("Building tree:");

			var fileTree = InfoBuilder.BuildInfo(args[0]);
			Console.WriteLine();

			Console.WriteLine("Building electoral map...");
			var map = new ElectoralMap(fileTree);

			Console.WriteLine();
			Console.WriteLine($"States: {map.StateCount}, counties: {map.CountyCount}");
			Console.WriteLine($"House seats: {map.TotalHouseVotes}, Senate seats: {map.TotalSenateVotes}");

			foreach (var party in map.GetPopulationByParty().Where(kvp => kvp.Value > 0).OrderByDescending(kvp => kvp.Value))
			{
				Console.WriteLine($"{party.Key}, {party.Value.ToString("N0")}");
			}

			Console.WriteLine();
			ElectionSimulator.SimulateElection(map);

			Console.ReadKey(intercept: true);
		}
	}
}
