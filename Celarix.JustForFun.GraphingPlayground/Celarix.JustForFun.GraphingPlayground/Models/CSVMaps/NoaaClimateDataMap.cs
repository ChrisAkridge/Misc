using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration;

namespace Celarix.JustForFun.GraphingPlayground.Models.CSVMaps
{
    internal sealed class NoaaClimateDataMap : ClassMap<NoaaClimateData>
    {
	    public NoaaClimateDataMap()
	    {
		    Map(m => m.StationId).Index(0);
			Map(m => m.Name).Index(1);
			Map(m => m.Date).Index(2);
			Map(m => m.TMax).Index(3);
			Map(m => m.TMin).Index(4);
		}
    }
}
