using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCompression
{
	public static class StringExtensions
	{
		public static string ToHexString(this byte[] input)
		{
			StringBuilder result = new StringBuilder(input.Length * 2);

			foreach (byte b in input)
			{
				result.Append($"{b:X2}");
			}

			return result.ToString();
		}
	}
}
