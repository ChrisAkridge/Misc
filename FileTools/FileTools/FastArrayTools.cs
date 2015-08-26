using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	internal static class FastArrayTools
	{
		public static unsafe bool FastContains8(byte[] data, byte searchFor)
		{
			fixed (byte* pb = &data[0])
			{
				for (byte* pi = pb; pi < (pb + data.Length); pi++)
				{
					if (*pi == searchFor) return true;
				}
			}

			return false;
		}

		public static unsafe bool FastContains16(byte[] data, ushort searchFor)
		{
			fixed (byte* pb = &data[0])
			{
				for (byte* pi = pb; pi < (pb + (data.Length - 1)); pi++)
				{
					if (*(ushort*)pi == searchFor) return true;
				}
			}

			return false;
		}

		public static unsafe bool FastContains24(byte[] data, uint searchFor)
		{
			byte ia = (byte)((searchFor & 0xFF0000) >> 16);
			byte ib = (byte)((searchFor & 0xFF00) >> 8);
			byte ic = (byte)(searchFor & 0xFF);

			fixed (byte* pb = &data[0])
			{
				for (byte* pi = pb; pi < (pb + (data.Length - 2)); pi++)
				{
					byte la = *pi;
					byte lb = *(pi + 1);
					byte lc = *(pi + 2);

					if (la == ia && lb == ib && lc == ic) return true;
				}
			}

			return false;
		}

		public static unsafe bool FastContains32(byte[] data, uint searchFor)
		{
			fixed (byte* pb = &data[0])
			{
				for (byte *pi = pb; pi < (pb + (data.Length - 3)); pi++)
				{
					if (*(uint*)pi == searchFor) return true;
				}
			}

			return false;
		}

		public static unsafe List<int> FastUnique16(byte[] data)
		{
			bool[] foundValue = new bool[65536];

			fixed (byte* pb = &data[0])
			{
				for (byte *pi = pb; pi < (pb + (data.Length - 1)); pi++)
				{
					ushort value = *(ushort*)pi;
					if (!foundValue[value]) foundValue[value] = true;
				}
			}

			var result = new List<int>();
			for (int i = 0; i < 65536; i++)
			{
				if (foundValue[i]) result.Add(i);
			}

			return result;
		}

		public static unsafe List<int> FastUnique24(byte[] data)
		{
			bool[] foundValue = new bool[16777216]; // why
			
			fixed (byte* pb = &data[0])
			{
				for (byte* pi = pb; pi < (pb + (data.Length - 2)); pi++)
				{
					byte a = *pi;
					byte b = *(pi + 1);
					byte c = *(pi + 2);
					uint value = (uint)((a << 16) + (b << 8) + c);

					if (!foundValue[value]) foundValue[value] = true;
				}
			}

			List<int> result = new List<int>();
			for (int i = 0; i < 16777216; i++)
			{
				if (foundValue[i]) result.Add(i);
			}

			return result;
		}

		public static unsafe IEnumerable<int> FastUnique32(byte[] data)
		{
			// bool[] foundValue = new bool[4294967296]... nah, I wouldn't be *that* cruel

			List<int> foundValues = new List<int>();
			fixed (byte* pb = &data[0])
			{
				for (byte* pi = pb; pi < (pb + (data.Length - 3)); pi++)
				{
					foundValues.Add(*(int*)pi);
				}
			}

			return foundValues.Distinct();
		}
	}
}
