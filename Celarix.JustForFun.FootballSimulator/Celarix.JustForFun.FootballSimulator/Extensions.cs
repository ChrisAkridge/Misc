﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator
{
    internal static class Extensions
    {
        public static int IndexOf<T>(this IEnumerable<T> sequence, T searchItem) where T : IEquatable<T>
        {
            var index = 0;

            foreach (var item in sequence)
            {
                if (item.Equals(searchItem))
                {
                    return index;
                }

                index += 1;
            }

            return -1;
        }

        // https://stackoverflow.com/a/69455054/2709212
        public static IList<T> Shuffle<T>(this IList<T> items, Random random)
        {
            for (int i = 0; i < items.Count - 1; i++)
            {
                int pos = random.Next(i, items.Count);
                (items[i], items[pos]) = (items[pos], items[i]);
            }

            return items;
        }
    }
}
