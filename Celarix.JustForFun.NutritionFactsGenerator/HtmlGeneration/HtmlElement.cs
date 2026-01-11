using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.NutritionFactsGenerator.HtmlGeneration
{
    internal sealed class HtmlElement(string elementType, string? innerText = null)
    {
        private readonly List<HtmlAttribute> attributes = new();
        private readonly List<HtmlElement> children = new();

        public string ElementType { get; } = elementType;
        public string? InnerText { get; } = innerText;
        public IReadOnlyList<HtmlAttribute> Attributes => attributes;
        public IReadOnlyList<HtmlElement> Children => children;

        public HtmlElement AddAttribute(string name, string value)
        {
            attributes.Add(new HtmlAttribute(name, value));
            return this;
        }

        public HtmlElement WithId(string id) => AddAttribute("id", id);
        public HtmlElement WithClass(string className) => AddAttribute("class", className);

        public HtmlElement AddChild(HtmlElement child)
        {
            children.Add(child);
            return this;
        }

        public string ToHtmlString(int depth = 0)
        {
            var indent = new string(' ', depth * 4);
            var sb = new StringBuilder();
            sb.Append($"{indent}<{ElementType}");
            foreach (var attribute in attributes)
            {
                sb.Append($" {attribute.ToHtmlString()}");
            }
            if (InnerText == null && children.Count == 0)
            {
                sb.Append(" />\n");
                return sb.ToString();
            }
            sb.Append('>');
            if (InnerText != null)
            {
                sb.Append(InnerText);
            }
            if (children.Count > 0)
            {
                sb.Append('\n');
                foreach (var child in children)
                {
                    sb.Append(child.ToHtmlString(depth + 1));
                }
                sb.Append($"{indent}");
            }
            sb.Append($"</{ElementType}>\n");
            return sb.ToString();
        }
    }
}
