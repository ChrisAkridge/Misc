using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal sealed class ConnectionGene : IGene
	{
		public int InnovationNumber { get; set; }

		public int FromIndex { get; internal set; }
		public int ToIndex { get; internal set; }
		public double Weight { get; internal set; }

		public IGene Clone()
		{
			var result = new ConnectionGene();

			result.FromIndex = FromIndex;
			result.ToIndex = ToIndex;
			result.Weight = Weight;

			return result;
		}
	}
}