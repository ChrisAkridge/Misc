using System;
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
	}
}
