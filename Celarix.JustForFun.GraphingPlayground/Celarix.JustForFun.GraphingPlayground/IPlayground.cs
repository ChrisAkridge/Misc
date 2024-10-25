﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models;
using ScottPlot.WinForms;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal interface IPlayground
	{
		string Name { get; }
		AdditionalSupport AdditionalSupport { get; }
		IReadOnlyList<PlotIndexMapping> IndexMappings { get; }


		void Load(PlaygroundLoadArguments loadArguments);
		Action<FormsPlot> GetView(string actionName);
		string[] GetViewNames();
	}
}
