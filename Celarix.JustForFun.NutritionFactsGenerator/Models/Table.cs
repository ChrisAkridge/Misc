using Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.Models
{
    internal sealed class Table
    {
        private readonly List<TableColumn> tableColumns = new();

        public string Id { get; }
        public string Header { get; }

        public Table(string id, string header)
        {
            Id = id;
            Header = header;
        }

        public IReadOnlyList<TableColumn> TableColumns => tableColumns;

        public Table AddColumn(string id, TableColumn column)
        {
            tableColumns.Add(column);
            return this;
        }

        public HtmlElement ToHtmlElement(string bootstrapBackgroundColorClass)
        {
            var outerDiv = new HtmlElement("div")
                .WithClass("col-12 panel-hidden");
            var innerDiv = new HtmlElement("div")
                .WithClass(bootstrapBackgroundColorClass);
            var headerElement = new HtmlElement("h2")
                .WithClass("text-center")
                .AddChild(new HtmlElement("span")
                    .WithClass("badge bg-secondary")
                    .AddChild(new HtmlElement("strong", Header)));
            innerDiv.AddChild(headerElement);
            var table = new HtmlElement("table").WithId(Id).WithClass("nutrition-facts-table");

            // Create thead
            var thead = new HtmlElement("thead");
            var headerRow = new HtmlElement("tr");
            var emptyColumnForRowLabels = new HtmlElement("th", "").WithClass("nutrition-facts-header");
            headerRow.AddChild(emptyColumnForRowLabels);
            foreach (var column in TableColumns)
            {
                var th = new HtmlElement("th", column.Label).WithClass("nutrition-facts-header");
                headerRow.AddChild(th);
            }
            thead.AddChild(headerRow);
            table.AddChild(thead);

            // Create tbody
            var rows = tableColumns.First().TableRows;
            var tbody = new HtmlElement("tbody");
            for (int r = 0; r < rows.Count; r++)
            {
                TableRow? row = rows[r];
                var tr = new HtmlElement("tr");
                tr.AddChild(new HtmlElement("td", row.Label).WithClass("nutrition-facts-row-label"));
                for (int c = 0; c < TableColumns.Count; c++)
                {
                    var td = new HtmlElement("td", "")
                        .WithId($"{Id}_{c}_{r}")
                        .WithClass("nutrition-facts-cell");
                    tr.AddChild(td);
                }
                tbody.AddChild(tr);
            }
            table.AddChild(tbody);
            innerDiv.AddChild(table);
            outerDiv.AddChild(innerDiv);

            return outerDiv;
        }

        public string GenerateOnUpdateJSStatements()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"    // Update table '{Id}'");
            for (int c = 0; c < TableColumns.Count; c++)
            {
                var column = TableColumns[c];
                if (column.DisplayIfExpression != null)
                {
                    builder.AppendLine($"    const {Id}_showColumn{c} = ({column.DisplayIfExpression});");
                    builder.AppendLine($"    for (let r = 0; r < {TableColumns[0].TableRows.Count}; r++) {{");
                    builder.AppendLine($"        const cell = document.getElementById('{Id}_{c}_' + r);");
                    builder.AppendLine($"        cell.style.display = {Id}_showColumn{c} ? '' : 'none';");
                    builder.AppendLine("    }");
                }
                for (int r = 0; r < column.TableRows.Count; r++)
                {
                    var row = column.TableRows[r];
                    var replaceValWith = $"({column.ExpressionValueName})";
                    var expression = row.Expression.Replace("val", replaceValWith);
                    if (FormatterUsesSubTags(row.FormatExpression))
                    {
                        builder.AppendLine($"    document.getElementById('{Id}_{c}_{r}').replaceChildren({row.FormatExpression}({expression}));");
                    }
                    else
                    {
                        builder.AppendLine($"    document.getElementById('{Id}_{c}_{r}').innerText = {row.FormatExpression}({expression});");
                    }
                }
            }
            return builder.ToString();
        }

        private static bool FormatterUsesSubTags(string formatExpression) =>
            formatExpression switch
            {
                "toPlanckMasses" => true,
                "toThermochemicalCalories" => true,
                "toPlanckEnergies" => true,
                "toPlanckLengths" => true,
                "toLunarMasses" => true,
                "toEarthMasses" => true,
                "toSolarMasses" => true,
                "toMilkyWayMasses" => true,
                "toUniverseMasses" => true,
                _ => false
            };
    }
}
