using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
    internal sealed class NoaaClimateData
    {
	    public string StationId { get; set; }
	    public string Name { get; set; }
	    public string Date { get; set; }
	    public string TMax { get; set; }
		public string TMin { get; set; }
	}
}
