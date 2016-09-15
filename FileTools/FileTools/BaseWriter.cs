using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FileTools
{
	internal static class BaseWriter
	{
		public static string BytesToBinaryString(byte[] data)
		{
			StringBuilder result = new StringBuilder(data.Length * 8);
			int i;

			foreach (byte b in data)
			{
				for (i = 0; i < 8; i++)
				{
					if ((b & (1 << i)) != 0)
					{
						result.Append("1");
					}
					else
					{
						result.Append("0");
					}
				}
			}

			return result.ToString();
		}

		public static string BytesToOctalString(byte[] data)
		{
			string hexString = BytesToHexString(data);
			StringBuilder result = new StringBuilder();

			for (int i = hexString.Length; i > 0; i -= 6)
			{
				string threeBytes;
				if (i < 6)
				{
					threeBytes = hexString.Substring(0, hexString.Length % 6);
				}
				else
				{
					threeBytes = hexString.Substring(i - 6, 6);
				}

				result.Append(Convert.ToString(Convert.ToInt32(threeBytes, 16), 8));
			}

			return result.ToString();
		}

		public static string BytesToDecimalString(byte[] data)
		{
			BigInteger number = new BigInteger(data);
			return number.ToString();
		}

		public static string BytesToHexString(byte[] data)
		{
			return string.Join("", data.Select(b => Convert.ToString(b, 16)));
		}

		public static string BytesToOffsetHexString(byte[] data)
		{
			string hexString = BytesToHexString(data);
			StringBuilder result = new StringBuilder();

			foreach (char c in hexString)
			{
				if (c >= '0' && c <= '9')
				{
					result.Append((char)('a' + (c - 0x30)));
				}
				else if (c >= 'a' && c <= 'f')
				{
					result.Append((char)('j' + (c - 0x61)));
				}
			}

			return result.ToString();
		}

		public static string BytesToBase64String(byte[] data)
		{
			return Convert.ToBase64String(data);
		}
	}
}
