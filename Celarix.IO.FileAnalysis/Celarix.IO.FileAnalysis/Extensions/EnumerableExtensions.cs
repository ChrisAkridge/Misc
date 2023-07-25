using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.IO.FileAnalysis.Collections;

namespace Celarix.IO.FileAnalysis.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Lazily groups a sorted sequence of items. Stops after returning each group.
        /// </summary>
        /// <typeparam name="TKey">The type of the key to group by.</typeparam>
        /// <typeparam name="TElement">The type of the element that groups are made of.</typeparam>
        /// <param name="sequence">The sequence of items to group.</param>
        /// <param name="keySelector">A function that selects the key from each item.</param>
        /// <returns>A grouped sequence of items.</returns>
        public static IEnumerable<IGrouping<TKey, TElement>> GroupOrderedBy<TKey, TElement>(this IEnumerable<TElement> sequence,
            Func<TElement, TKey> keySelector) where TKey : IComparable<TKey>
        {
            var onFirstElement = true;
            TKey lastKey = default;
            var currentGroupElements = new List<TElement>();

            foreach (var element in sequence)
            {
                var key = keySelector(element);

                if (onFirstElement)
                {
                    lastKey = key;
                    currentGroupElements.Add(element);
                    onFirstElement = false;
                }
                else
                {
                    var comparisonToLastKey = lastKey.CompareTo(key);
                    lastKey = key;

                    if (comparisonToLastKey == 0)
                    {
                        // This key is the same as the last one, append to current group.
                        currentGroupElements.Add(element);
                    }
                    else if (comparisonToLastKey < 0)
                    {
                        // This is a new key, so we need to return a group for the last key and start a new one.
                        var group = new Grouping<TKey, TElement>(key, currentGroupElements);
                        currentGroupElements = new List<TElement>
                        {
                            element
                        };

                        yield return group;
                    }
                    else
                    {
                        // This key comes before the previous key, which means
                        // this sequence is not sorted.
                        throw new ArgumentException("Cannot lazily group a non-sorted sequence; use GroupBy(), or sort the sequence by key, instead");
                    }
                }
            }
        }
    }
}
