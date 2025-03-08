using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models.Html
{
    internal sealed class HtmlElement
    {
        public string TagName { get; set; }
        public List<HtmlAttribute> Attributes { get; set; }
        public List<HtmlElement> Children { get; set; }
        public string InnerText { get; set; }

        public HtmlElement(string tagName)
        {
	        TagName = tagName;
	        Attributes = new List<HtmlAttribute>();
	        Children = new List<HtmlElement>();
        }

        public string PrintSelf(int depth)
        {
	        Debug.Print($"Printing {TagName} at depth {depth}");
	        
	        var builder = new StringBuilder();
	        var tabs = new string(' ', depth * 4);
	        var attributes = "";

	        if (Attributes.Count > 0)
	        {
		        attributes = " "
			        + string.Join(' ',
						Attributes.Select(a => $"{a.Name}=\"{a.Value}\""));
	        }

	        var openingTag = $"<{TagName}{attributes}>";
	        var closingTag = $"</{TagName}>";

	        builder.AppendLine(tabs + openingTag);

	        if (Children.Count > 0)
	        {
		        foreach (var child in Children)
		        {
			        builder.AppendLine(child.PrintSelf(depth + 1));
		        }
	        }
	        else
	        {
		        builder.AppendLine(tabs + InnerText);
	        }
	        
	        builder.AppendLine(tabs + closingTag);
	        return builder.ToString();
        }
    }
}
