using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterManager
{
	public sealed class CustomCharacter
	{
		public byte[] Pattern { get; private set; }

		public CustomCharacter(byte a, byte b, byte c, byte d, byte e, byte f, byte g, byte h)
		{
			Pattern = new byte[] { a, b, c, d, e, f, g, h };
		}

		public CustomCharacter(byte[] pattern)
		{
			if (pattern == null || pattern.Length != 8)
			{
				throw new ArgumentException();
			}
			Pattern = pattern;
		}

		public byte[] GenerateLoadBytes(int slot)
		{
			if (slot < 0 || slot > 7)
			{
				throw new ArgumentOutOfRangeException();
			}

			byte[] result = new byte[11];
			result[0] = 0xFE;
			result[1] = 0x4E;
			result[2] = (byte)slot;

			for (int i = 0; i < 8; i++)
			{
				result[i + 3] = Pattern[i];
			}

			return result;
		}
	}
}
