using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FileAnalysisIII
{
	public sealed class BSPTreeNode<T>(T rangeMinimum, T rangeMaximum)
		where T : struct,
		INumber<T>,
		IAdditionOperators<T, int, T>,
		IDivisionOperators<T, int, T>,
		IMinMaxValue<T>
	{
#nullable enable
		private const int MaxLeafValues = 256;
		
		private readonly T rangeMinimum = rangeMinimum;
		private readonly T rangeMaximum = rangeMaximum;

		private KeyValuePair<T, int>?[]? leafValues = new KeyValuePair<T, int>?[MaxLeafValues];
		private static readonly KeyValuePair<T, int>[] treeSplitBuffer = new KeyValuePair<T, int>[MaxLeafValues + 1];
		private int leafValueCount;
		private BSPTreeNode<T>? left;
		private BSPTreeNode<T>? right;
		
		public int Count =>
			left != null
				? left.Count + right!.Count
				: leafValueCount;

		public void Add(T item, int count = 1)
		{
			if (left != null && right != null)
			{
				// We've already split. Figure out which half it goes in and tell that node to add it.
				if (item.CompareTo(right.rangeMinimum) <= 0)
				{
					left.Add(item, count);
				}
				else
				{
					right.Add(item, count);
				}
			}
			else if (Count <= MaxLeafValues)
			{
				// This is a leaf node and contains the actual values. Either find it or add it.
				int foundIndex = -1;
				for (var i = 0; i < leafValueCount; i++)
				{
					if (leafValues![i]!.Value.Key.CompareTo(item) != 0) { continue; }

					foundIndex = i;
					break;
				}
				
				if (foundIndex != -1)
				{
					// We found the item. Increment the count.
					leafValues![foundIndex] = new KeyValuePair<T, int>(leafValues[foundIndex]!.Value.Key, leafValues[foundIndex]!.Value.Value + count);
				}
				else
				{
					// We didn't find the item. Try to add it.
					if (Count < MaxLeafValues)
					{
						leafValues![Count] = new KeyValuePair<T, int>(item, count);
						leafValueCount++;
					}
					else
					{
						// We have reached the limit of this leaf node. We need to split it.
						for (var i = 0; i < MaxLeafValues; i++)
						{
							treeSplitBuffer[i] = leafValues![i]!.Value;
						}
						treeSplitBuffer[MaxLeafValues] = new KeyValuePair<T, int>(item, count);
						Array.Sort(treeSplitBuffer, (a, b) => a.Key.CompareTo(b.Key));
						var leftRangeMaximum = ((rangeMaximum - rangeMinimum) / 2) + rangeMinimum;
						var rightRangeMinimum = leftRangeMaximum + 1;
						
						left = new BSPTreeNode<T>(rangeMinimum, leftRangeMaximum);
						right = new BSPTreeNode<T>(rightRangeMinimum, rangeMaximum);
						
						for (var i = 0; i < MaxLeafValues + 1; i++)
						{
							var itemToAdd = treeSplitBuffer[i];
							if (itemToAdd.Key.CompareTo(leftRangeMaximum) <= 0)
							{
								left.Add(itemToAdd.Key, itemToAdd.Value);
							}
							else
							{
								right.Add(itemToAdd.Key, itemToAdd.Value);
							}
						}

						leafValues = null;
					}
				}
			}
		}
#nullable disable
	}
}
