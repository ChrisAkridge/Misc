using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Collections
{
	internal sealed class SparseMaxArray<T> : IEnumerable<T?>
	{
		private readonly Dictionary<int, T?> values = new();

		public T? this[int index]
		{
			get
			{
				if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative."); }
				return values.GetValueOrDefault(index);
			}
			set
			{
				if (index < 0) { throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative."); }
				if (EqualityComparer<T>.Default.Equals(value, default))
				{
					values.Remove(index);
				}
				else
				{
					values[index] = value;
				}
			}
		}

		public int Count => values.Keys.Max();

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<T?> GetEnumerator()
		{
			var maxIndex = values.Keys.Max();

			for (var i = 0; i < maxIndex; i++)
			{
				yield return this[i];
			}
		}

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
