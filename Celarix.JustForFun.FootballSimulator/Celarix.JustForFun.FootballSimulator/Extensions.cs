using System;
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
    }
}
