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
		private static readonly KeyValuePair<T, int>[] treeSplitBuffer = new KeyValuePair<T, int>[5];
		private static readonly T?[] valueSearchBuffer = new T?[4];

		private BSPTreeNode<T>? left;
		private BSPTreeNode<T>? right;

		private KeyValuePair<T, int>? _0;
		private KeyValuePair<T, int>? _1;
		private KeyValuePair<T, int>? _2;
		private KeyValuePair<T, int>? _3;
		private readonly T rangeMinimum = rangeMinimum;
		private readonly T rangeMaximum = rangeMaximum;

		public int Count
		{
			get
			{
				if (left != null)
				{
					return left.Count + right!.Count;
				}
				
				if (_3 != null) { return 4; }
				if (_2 != null) { return 3; }
				if (_1 != null) { return 2; }
				if (_0 != null) { return 1; }
				return 0;
			}
		}

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
			else if (Count <= 4)
			{
				// This is a leaf node and contains the actual values. Either find it or add it.
				// First, do a null check.
				Array.Clear(valueSearchBuffer);
				if (_0 != null) { valueSearchBuffer[0] = _0!.Value.Key; }
				if (_1 != null) { valueSearchBuffer[1] = _1!.Value.Key; }
				if (_2 != null) { valueSearchBuffer[2] = _2!.Value.Key; }
				if (_3 != null) { valueSearchBuffer[3] = _3!.Value.Key; }
				var foundIndex = Array.IndexOf(valueSearchBuffer, item);
				if (foundIndex != -1)
				{
					// We found the item. Increment the count.
					switch (foundIndex)
					{
						case 0:
							_0 = new KeyValuePair<T, int>(_0!.Value.Key, _0.Value.Value + count);
							break;
						case 1:
							_1 = new KeyValuePair<T, int>(_1!.Value.Key, _1.Value.Value + count);
							break;
						case 2:
							_2 = new KeyValuePair<T, int>(_2!.Value.Key, _2.Value.Value + count);
							break;
						case 3:
							_3 = new KeyValuePair<T, int>(_3!.Value.Key, _3.Value.Value + count);
							break;
					}
				}
				else
				{
					// We didn't find the item. Try to add it.
					if (Count < 4)
					{
						if (_0 == null) { _0 = new KeyValuePair<T, int>(item, count); }
						else if (_1 == null) { _1 = new KeyValuePair<T, int>(item, count); }
						else if (_2 == null) { _2 = new KeyValuePair<T, int>(item, count); }
						else if (_3 == null) { _3 = new KeyValuePair<T, int>(item, count); }
					}
					else
					{
						// We have reached the limit of this leaf node. We need to split it.
						treeSplitBuffer[0] = _0!.Value;
						treeSplitBuffer[1] = _1!.Value;
						treeSplitBuffer[2] = _2!.Value;
						treeSplitBuffer[3] = _3!.Value;
						treeSplitBuffer[4] = new KeyValuePair<T, int>(item, count);
						
						Array.Sort(treeSplitBuffer, (a, b) => a.Key.CompareTo(b.Key));
						var leftRangeMaximum = ((rangeMaximum - rangeMinimum) / 2) + rangeMinimum;
						var rightRangeMinimum = leftRangeMaximum + 1;
						
						left = new BSPTreeNode<T>(rangeMinimum, leftRangeMaximum);
						right = new BSPTreeNode<T>(rightRangeMinimum, rangeMaximum);
						
						for (var i = 0; i < 5; i++)
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
						
						// Clear out the leaf node.
						_0 = null;
						_1 = null;
						_2 = null;
						_3 = null;
					}
				}
			}
		}
#nullable disable
	}
}
