using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryOfNoise
{
	public static class NoiseGenerator
	{
		private static Random randomA;
		private static Random randomB;

		private static uint currentASeed;
		private static uint currentBSeed;
		private static ulong currentSetNumber;

		private static string lookupTable;

		static NoiseGenerator()
		{
			lookupTable = "abcdefghijklmnopqrstuvwxyz .,!?:";
		}

		public static string GeneratePage(ulong set, ulong page)
		{
			uint high = (uint)(page >> 32);
			uint low = unchecked((uint)(page & 0xFFFFFFFF) + 1);

			currentASeed = high;
			currentBSeed = low;
			currentSetNumber = set;

			randomA = new Random(unchecked((int)high));
			randomB = new Random(unchecked((int)low));

			StringBuilder result = new StringBuilder(1024);

			for (int i = 0; i < 512; i++)
			{
				int charIndexA = randomA.Next(0, 32);
				int charIndexB = randomB.Next(0, 32);

				int charIndex = (charIndexA << 5) + charIndexB;

				int setByteNumber = i % 8;
				byte setByte = (byte)(set & (0xFFul << (setByteNumber * 8)));

				charIndex |= setByte;
				charIndex &= 0x3FF;

				char a = lookupTable[(charIndex >> 5)];
				char b = lookupTable[(charIndex & 0x1f)];

				result.Append(a);
				result.Append(b);
			}

			return result.ToString();
		}
	}
}
