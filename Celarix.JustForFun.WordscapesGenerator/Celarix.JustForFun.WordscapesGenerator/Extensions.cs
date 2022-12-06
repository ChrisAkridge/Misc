using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WordscapesGenerator
{
    public static class Extensions
    {
        // https://stackoverflow.com/a/2776689
        public static string Truncate(this string value, int maxLength) =>
            string.IsNullOrEmpty(value)
                ? value
                : value.Length <= maxLength
                    ? value
                    : value[..maxLength];
    }
}
