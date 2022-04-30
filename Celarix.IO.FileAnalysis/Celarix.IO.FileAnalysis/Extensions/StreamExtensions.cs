using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Extensions
{
    public static class StreamExtensions
    {
        public static void CopyTo(this StreamReader source, StreamWriter destination, int bufferSize = 81920)
        {
            // https://stackoverflow.com/a/17390783/2709212
            // because .NET 5 is weird

            var array = new char[bufferSize];
            int count;

            while ((count = source.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }
    }
}
