using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingPuzzles
{
	public abstract class Puzzle
	{
		public abstract string Description { get; }
		public Puzzle() { }

		public abstract void Run();
	}
}
