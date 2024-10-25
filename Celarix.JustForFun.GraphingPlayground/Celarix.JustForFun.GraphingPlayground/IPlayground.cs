using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal interface IPlayground
	{
		string Name { get; }
		
		void Load(PlaygroundLoadArguments loadArguments);
		Action<FormsPlot> GetView(string actionName);
		string[] GetViewNames();
	}
}
