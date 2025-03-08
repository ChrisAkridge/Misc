using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class MeijerWeek
	{
		public int WeekNumber { get; init; }
		public int WorkedDays { get; init; }
		public int OffDays { get; init; }
	}
}
