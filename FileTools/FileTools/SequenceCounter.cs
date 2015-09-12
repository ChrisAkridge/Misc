using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	public static class SequenceCounter
	{
		public static Tuple<long, long> CountBits(byte[] data)
		{
			long totalBits = data.Length * 8L;
			long setBits = 0L;

			foreach (byte b in data)
			{
				for (int i = 0; i < 7; i++)
				{
					setBits += ((b & (1 << i)) != 0) ? 1 : 0;
				}
			}

			return new Tuple<long, long>(totalBits - setBits, setBits);
		}

		public static long[] CountBitPairs(byte[] data)
		{
			long[] result = new long[4];

			foreach (byte b in data)
			{
				for (int i = 0; i < 7; i += 2)
				{
					int bitPair = (b & 3 << i) >> i;
					result[bitPair]++;
				}
			}

			return result;
		}

		public static long[] CountNybbles(byte[] data)
		{
			long[] result = new long[16];

			foreach (byte b in data)
			{
				result[(b & 0xF0) >> 4]++;
				result[(b & 0x0F)]++;
			}

			return result;
		}

		public static long[] CountBytes(byte[] data)
		{
			long[] result = new long[256];

			foreach (byte b in data)
			{
				result[b]++;
			}

			return result;
		}

		public static Dictionary<ushort, int> CountWords(byte[] data)
		{
			var result = new Dictionary<ushort, int>();

			for (int i = 0; i < data.Length; i += 2)
			{
				byte high = data[i];
				byte low = (byte)((i < data.Length - 1) ? data[i + 1] : 0);

				ushort word = (ushort)((high << 8) | low);

				if (!result.ContainsKey(word))
				{
					result.Add(word, 1);
				}
				else
				{
					result[word]++;
				}
			}

			return result;
		}

		public unsafe static Dictionary<uint, int> CountDWords(byte[] data)
		{
			var result = new Dictionary<uint, int>();
			int newLength = 0;

			fixed (byte* pb = &data[0])
			{
				byte* endOfArray = (pb + data.Length);
				int bytesToWrite = 4 - (data.Length % 4);

				for (int i = 0; i < bytesToWrite; i++)
				{
					*(endOfArray + i) = 0x00; // "I dunno why it's crashing randomly... Can't be the overwriting memory bit... nope, not at all..."
				}

				newLength = (data.Length + bytesToWrite) / 4;
			}

			fixed (byte* pb = &data[0])
			{
				uint* pi = (uint*)pb;

				for (int i = 0; i < newLength; i++)
				{
					if (!result.ContainsKey(*(pi + i)))
					{
						result.Add(*(pi + i), 1);
					}
					else
					{
						result[*(pi + i)]++;
					}
				}
			}

			return result;
		}

		public unsafe static Dictionary<ulong, int> CountQWords(byte[] data)
		{
			var result = new Dictionary<ulong, int>();
			int newLength = 0;

			fixed (byte* pb = &data[0])
			{
				byte* endOfArray = (pb + data.Length);
				int bytesToWrite = 8 - (data.Length % 8);

				for (int i = 0; i < bytesToWrite; i++)
				{
					*(endOfArray + i) = 0x00;
				}

				newLength = (data.Length + bytesToWrite) / 8;
			}

			fixed (byte* pb = &data[0])
			{
				ulong* pl = (ulong*)pb;

				for (int i = 0; i < newLength; i++)
				{
					if (!result.ContainsKey(*(pl + i)))
					{
						result.Add(*(pl + i), 1);
					}
					else
					{
						result[*(pl + i)]++;
					}
				}
			}

			return result;
		}
	}
}
