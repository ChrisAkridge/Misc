using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration
{
    internal sealed class HtmlAttribute(string name, string value)
    {
        public string Name { get; } = name;
        public string Value { get; } = value;

        public string ToHtmlString() => $"{Name}=\"{Value}\"";
    }
}
