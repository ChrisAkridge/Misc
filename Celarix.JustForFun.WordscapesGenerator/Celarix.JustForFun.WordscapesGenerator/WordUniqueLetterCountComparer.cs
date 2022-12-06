using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WordscapesGenerator
{
    public sealed class WordUniqueLetterCountComparer : IComparer<string>
    {
        /// <summary>Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.</summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description><paramref name="x" /> is less than <paramref name="y" />.</description></item><item><term> Zero</term><description><paramref name="x" /> equals <paramref name="y" />.</description></item><item><term> Greater than zero</term><description><paramref name="x" /> is greater than <paramref name="y" />.</description></item></list></returns>
        public int Compare(string? x, string? y)
        {
            switch (x)
            {
                case null when y == null:
                    return 0;
                case null:
                    return -1;
                default:
                {
                    if (y == null)
                    {
                        return 1;
                    }
                    break;
                }
            }
            
            return x.Distinct().Count().CompareTo(y.Distinct().Count());
        }
    }
}
