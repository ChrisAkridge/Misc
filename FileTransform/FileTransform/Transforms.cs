using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransform
{
	public static class Transforms
	{
		private static IEnumerable<Tuple<byte, byte>> GenerateBytePairSource(IEnumerable<byte> source)
		{
			using (IEnumerator<byte> enumerator = source.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					byte a = enumerator.Current;
					if (enumerator.MoveNext())
					{
						byte b = enumerator.Current;
						yield return Tuple.Create(a, b);
					}
					else
					{
						yield return Tuple.Create<byte, byte>(a, 0);
					}
				}
			}
		}

		public static IEnumerable<byte> Inverse(IEnumerable<byte> source)
		{
			foreach (byte value in source)
			{
				byte result = unchecked((byte)-(sbyte)value);
				yield return result;
			}
		}

		public static IEnumerable<byte> BitwiseNot(IEnumerable<byte> source)
		{
			foreach (byte value in source)
			{
				byte result = (byte)~value;
				yield return result;
			}
		}

		public static IEnumerable<byte> Factorial(IEnumerable<byte> source)
		{
			foreach (byte value in source)
			{
				byte i = value;
				byte result = 1;
				while (i > 0)
				{
					unchecked
					{
						result *= i;
					}
					i--;
				}

				yield return result;
			}
		}

		public static IEnumerable<byte> LogicalNot(IEnumerable<byte> source)
		{
			foreach (byte value in source)
			{
				yield return (value == 0) ? (byte)1 : (byte)0;
			}
		}

		public static IEnumerable<byte> Add(IEnumerable<byte> source)
		{
			var pairSource = GenerateBytePairSource(source);

			foreach (Tuple<byte, byte> value in pairSource)
			{
				yield return unchecked((byte)(value.Item1 + value.Item2));
			}
		}

		public static IEnumerable<byte> Subtract(IEnumerable<byte> source)
		{
			var pairSource = GenerateBytePairSource(source);

			foreach (var value in pairSource)
			{
				yield return unchecked((byte)(value.Item1 - value.Item2));
			}
		}
	}
}
