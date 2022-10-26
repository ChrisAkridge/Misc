using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea
{
    internal static class Extensions
    {
        // https://stackoverflow.com/a/19793543
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    return index;
                }
                
                index++;
            }

            return -1;
        }
    }
}
