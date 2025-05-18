using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.ReceiptPrinter.Extensions
{
    internal static class StringBuilderExtensions
    {
        public static bool All(this StringBuilder builder, Func<char, bool> predicate)
        {
            for (var i = 0; i < builder.Length; i++)
            {
                if (!predicate(builder[i]))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
