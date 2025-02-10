using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Simulation
{
	internal sealed class SimulationItem(string itemReference, long quantity)
	{
		public string ItemReference { get; } = itemReference;
		public long Quantity { get; } = quantity;
		
		public override string ToString() => $"{Quantity}x {ItemReference}";
	}
}
