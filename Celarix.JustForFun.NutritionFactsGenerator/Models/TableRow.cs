using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed class TableRow(string label, string expression, string formatFunction)
    {
        public string Label { get; } = label;
        public string Expression { get; } = expression;
        public string FormatExpression { get; } = formatFunction;
    }
}
