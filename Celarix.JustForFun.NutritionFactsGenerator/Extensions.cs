using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator
{
    internal static class Extensions
    {
        public static string WithSubscript(this string text, string subscript)
        {
            return $"{text}<sub>{subscript}</sub>";
        }

        public static string CapitalizeFirstLetter(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            if (text.Length == 1)
            {
                return text.ToUpper();
            }
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}
