using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Simulation
{
	internal sealed class SimulationResult(
		IReadOnlyList<SimulationStep> steps)
	{
		public IReadOnlyList<SimulationStep> Steps => steps;
		public long TotalSmeltingCost => Steps.Sum(s => s.StepSmeltingCost);
		public long TotalCompressionCost => Steps.Sum(s => s.StepCompressionCost);
		public long TotalHypercompressionCost => Steps.Sum(s => s.StepHypercompressionCost);
		public decimal TotalMiningTime => Steps.Sum(s => s.StepMiningTime);
		public long TotalKillingCost => Steps.Sum(s => s.StepKillingCost);
	}
}
