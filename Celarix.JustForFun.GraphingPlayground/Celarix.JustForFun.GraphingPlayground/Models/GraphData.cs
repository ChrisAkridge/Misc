using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.GraphingPlayground.Models
{
	internal sealed class GraphData<TXAxis, TYAxis>
	{
		private readonly TXAxis[] xValues;
		private readonly TYAxis[] yValues;
		
		public IReadOnlyList<TXAxis> XValues => xValues;
		public IReadOnlyList<TYAxis> YValues => yValues;
		public string Title { get; init; }
		public string XAxisLabel { get; init; }
		public string YAxisLabel { get; init; }
		public GraphType GraphType { get; init; }
		
		public GraphData(TXAxis[] xValues, TYAxis[] yValues, string title, string xAxisLabel, string yAxisLabel,
			GraphType graphType)
		{
			this.xValues = xValues;
			this.yValues = yValues;
			Title = title;
			XAxisLabel = xAxisLabel;
			YAxisLabel = yAxisLabel;
			GraphType = graphType;
		}
	}
}
