using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingPuzzles.Puzzles
{
	public class Disemvoweler : Puzzle
	{
		// From http://www.reddit.com/r/dailyprogrammer/comments/1ystvb/022414_challenge_149_easy_disemvoweler/

		public override string Description
		{
			get
			{
				return "Removes all vowels from an input string and prints the result, plus all the vowels removed.";
			}
		}

		public override void Run()
		{
			Console.WriteLine("Input:");
			string input = new string(Console.ReadLine().ToLowerInvariant().Where(c => char.IsLetter(c)).ToArray());

			char[] vowels = new char[] { 'a', 'e', 'i', 'o', 'u' };
			string inputNoVowels = new string(input.Where(c => !vowels.Contains(c)).ToArray());
			string inputVowels = new string(input.Where(c => vowels.Contains(c)).ToArray());

			Console.WriteLine(inputNoVowels);
			Console.WriteLine(inputVowels);
		} 
	}
}
