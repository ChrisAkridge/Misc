using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using ScottPlot.TickGenerators;

namespace Celarix.JustForFun.GraphingPlayground.Logic.MeijerDeepStatistics
{
    internal static class PlotGenerator
    {
	    public static Plot BarsWithManualTicks(string title, List<KeyValuePair<string, double>> namedValues)
	    {
		    var plot = new Plot();
		    var position = 1;
		    var ticks = new List<Tick>();

		    foreach (var namedValue in namedValues)
		    {
			    var bar = new Bar
			    {
					Position = position,
					Value = namedValue.Value,
					Label = $"{namedValue.Value}",
					CenterLabel = true
				};
			    plot.Add.Bar(bar);
			    ticks.Add(new(position, namedValue.Key));
		    }

		    plot.Axes.Bottom.TickGenerator = new NumericManual(ticks.ToArray());
		    plot.Axes.Bottom.MajorTickStyle.Length = 0;
		    plot.HideGrid();
		    
		    plot.Axes.Margins(bottom: 0);
		    plot.Title(title);

		    return plot;
	    }
    }
}
