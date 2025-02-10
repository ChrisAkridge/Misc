using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Simulation
{
	internal sealed class SimulationStep(IReadOnlyList<SimulationItem> nonTerminalItems,
		IReadOnlyList<SimulationItem> minedItems,
		IReadOnlyList<SimulationItem> killedItems,
		long stepHypercompressionCost,
		long stepCompressionCost,
		long stepKillingCost,
		decimal stepMiningTime,
		long stepSmeltingCost)
	{
		public long StepHypercompressionCost { get; } = stepHypercompressionCost;
		public long StepCompressionCost { get; } = stepCompressionCost;
		public long StepKillingCost { get; } = stepKillingCost;
		public decimal StepMiningTime { get; } = stepMiningTime;
		public long StepSmeltingCost { get; } = stepSmeltingCost;

		public bool AllItemsTerminal => nonTerminalItems.Count == 0;
		public IReadOnlyList<SimulationItem> NonTerminalItems => nonTerminalItems;
		public IReadOnlyList<SimulationItem> MinedItems => minedItems;
		public IReadOnlyList<SimulationItem> KilledItems => killedItems;
		
		public IEnumerable<SimulationItem> AllItems => nonTerminalItems.Concat(minedItems).Concat(killedItems);
	}
}
