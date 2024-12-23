﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal static class Extensions
	{
		public static void RemoveAtIndices<T>(this IList<T> list, int[] indices)
		{
			// Remove the indices in reverse order to avoid shifting elements
			Array.Sort(indices);
			for (int i = indices.Length - 1; i >= 0; i--)
			{
				list.RemoveAt(indices[i]);
			}
		}

		public static string Ordinal(this int number) =>
			number switch
			{
				1 => "1st",
				2 => "2nd",
				3 => "3rd",
				_ => number + "th"
			};
		
		public static IEnumerable<(T? First, T? Second)> Pair<T>(this IEnumerable<T?> source)
		{
			using var enumerator = source.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var first = enumerator.Current;
				if (!enumerator.MoveNext())
				{
					yield return (first, default);
				}

				var second = enumerator.Current;
				yield return (first, second);
			}
		}

		public static T[] Flatten2DArray<T>(this T[,] array)
		{
			var flatArray = new T[array.GetLength(0) * array.GetLength(1)];
			for (int i = 0; i < array.GetLength(0); i++)
			{
				for (int j = 0; j < array.GetLength(1); j++)
				{
					flatArray[(i * array.GetLength(1)) + j] = array[i, j];
				}
			}

			return flatArray;
		}
	}
}
