using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models.Html
{
	internal sealed class HtmlAttribute
	{
		public string Name { get; set; }
		public string Value { get; set; }
		
		public HtmlAttribute(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}
}
