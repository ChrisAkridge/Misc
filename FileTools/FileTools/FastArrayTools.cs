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
	}
}
