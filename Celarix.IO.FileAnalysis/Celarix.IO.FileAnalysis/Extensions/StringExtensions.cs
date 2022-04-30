using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Extensions
{
    public static class StringExtensions
    {
        public static string WithoutEndingPathSeparator(this string path) => path.TrimEnd(Pri.LongPath.Path.DirectorySeparatorChar);
    }
}
