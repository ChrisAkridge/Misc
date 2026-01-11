using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using Celarix.JustForFun.NutritionFactsGenerator.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed class ComputedDescription(string htmlElementId,
        string displayName,
        int groupNumber,
        string valueExpression,
        string? unitName = null,
        bool? unitIsComputed = null) : IInputRow
    {
        public string HtmlElementId { get; } = htmlElementId;
        public string DisplayName { get; } = displayName;
        public int GroupNumber { get; } = groupNumber;
        public string ValueExpression { get; } = valueExpression;
        public string? UnitName { get; } = unitName;
        public bool UnitIsComputed { get; } = unitIsComputed ?? false;

        public HtmlElement ToElement()
        {
            var groupParity = $"grid-block-{GroupNumber % 2}";
            var fullDisplayName = DisplayName;
            if (UnitName != null && !UnitIsComputed)
            {
                fullDisplayName += $" ({UnitName})";
            }
            var tr = new HtmlElement("tr");
            tr.WithClass(groupParity);
            tr.AddChild(new HtmlElement("td")
                .AddChild(new HtmlElement("span", fullDisplayName)));
            tr.AddChild(new HtmlElement("td")
                .AddChild(new HtmlElement("span")
                    .WithClass("calc")
                    .WithId($"calc-{HtmlElementId}")));
            return tr;
        }

        public string OnUpdateJsStatements()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"    // {HtmlElementId}");
            builder.AppendLine($"    const {HtmlElementId}Value = ({ValueExpression});");
            if (UnitIsComputed)
            {
                builder.AppendLine($"    const {HtmlElementId}Unit = ' ' + ({UnitName!});");
            }
            else if (UnitName != null)
            {
                builder.AppendLine($"    const {HtmlElementId}Unit = ' {UnitName}'");
            }
            else
            {
                builder.AppendLine($"    const {HtmlElementId}Unit = ''");
            }
            var spanId = $"calc-{HtmlElementId}";
            builder.AppendLine($"    document.getElementById('{spanId}').innerText = {HtmlElementId}Value.toFixed(3) + {HtmlElementId}Unit;");
            return builder.ToString();
        }
    }
}
