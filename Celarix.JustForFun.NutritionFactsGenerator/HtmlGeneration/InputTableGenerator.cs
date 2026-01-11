using Celarix.JustForFun.NutritionFactsGenerator.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration
{
    internal static class InputTableGenerator
    {
        public static HtmlElement GenerateFromInputRows(IEnumerable<IInputRow> rows,
            string bootstrapBackgroundColorClass,
            string headerText)
        {
            var outerDiv = new HtmlElement("div")
                .WithClass("col-12 panel-hidden");
            var innerDiv = new HtmlElement("div")
                .WithClass(bootstrapBackgroundColorClass);
            var h2 = new HtmlElement("h2")
                .WithClass("text-center")
                .AddChild(new HtmlElement("span")
                    .WithClass("badge bg-secondary")
                    .AddChild(new HtmlElement("strong", headerText)));
            var table = new HtmlElement("table")
                .WithClass("table table-sm table-bordered mb-0");
            var tbody = new HtmlElement("tbody");
            foreach (var row in rows)
            {
                var tr = row.ToElement();
                tbody.AddChild(tr);
            }
            table.AddChild(tbody);
            innerDiv.AddChild(h2);
            innerDiv.AddChild(table);
            outerDiv.AddChild(innerDiv);
            return outerDiv;
        }
    }
}
