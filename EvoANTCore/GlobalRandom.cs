using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public static class GlobalRandom
	{
		private static Random random = new Random();

		public static int Next() => random.Next();
		public static int Next(int maxValue) => random.Next(maxValue);
		public static int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
		public static double NextDouble() => random.NextDouble();

		public static T ChooseRandom<T>(IEnumerable<T> sequence)
		{
			int count = sequence.Count();
			if (count == 0) { return default(T); }
			return sequence.Skip(random.Next(0, count)).First();
		}

		public static Tuple<T, T> ChooseRandomPair<T>(IEnumerable<T> sequence) where T : class
		{
			int count = sequence.Count();
			if (count == 0) { return Tuple.Create<T, T>(null, null); }
			else if (count == 1) { return Tuple.Create<T, T>(sequence.First(), null); }
			else if (count == 2) { return Tuple.Create(sequence.First(), sequence.Last()); }

			int index1 = random.Next(0, count);
			int index2 = random.Next(0, count);
			while (index2 == index1) { index2 = random.Next(0, count); }

			T item1 = sequence.Skip(index1).First();
			T item2 = sequence.Skip(index2).First();

			return Tuple.Create(item1, item2);
		}
	}
}
