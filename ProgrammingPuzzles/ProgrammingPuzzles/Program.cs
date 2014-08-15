using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProgrammingPuzzles.Puzzles;

namespace ProgrammingPuzzles
{
	class Program
	{
		private static Dictionary<string, Puzzle> puzzles;

		static void Main(string[] args)
		{
			InstantiatePuzzles();

			Console.WriteLine("Celarian Programming Puzzle Implementations:");
			Console.WriteLine("Taken from various sources, including the Daily Programmer subreddit");

			input:
			Console.WriteLine("Enter ? to see all puzzles, or a puzzle name to run:");
			string selection = Console.ReadLine();
			
			if (selection == "?")
			{
				PrintAllPuzzles();
				goto input;
			}
			else
			{
				if (!puzzles.ContainsKey(selection))
				{
					Console.WriteLine("There's no puzzle with that name. Try again, please.");
					goto input;
				}

				Console.Clear();
				puzzles[selection].Run();
				Console.ReadKey();
			}
		}

		private static void InstantiatePuzzles()
		{
			puzzles = new Dictionary<string, Puzzle>();

			// Add new puzzles below
			// Example: puzzles.Add("myPuzzle", new MyPuzzle());
			puzzles.Add("disemvoweler", new Disemvoweler());
			puzzles.Add("langtonsant", new LangtonsAnt());
		}

		private static void PrintAllPuzzles()
		{
			foreach (var puzzle in puzzles)
			{
				Console.WriteLine("{0}: {1}", puzzle.Key, puzzle.Value.Description);
			}
		}
	}
}
