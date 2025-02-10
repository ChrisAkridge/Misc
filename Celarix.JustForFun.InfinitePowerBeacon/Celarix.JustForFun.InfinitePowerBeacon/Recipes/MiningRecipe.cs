using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class MiningRecipe(string blockReference, bool requiresPickaxe, MiningTier requiredMiningTier, decimal miningTime, int minimumDrops, int maximumDrops)
		: Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Mining;
		
		public string BlockReference { get; } = blockReference;

		public bool RequiresPickaxe { get; } = requiresPickaxe;
		public MiningTier RequiredMiningTier { get; } = requiredMiningTier;
		// Measured with a proper Netherite tool with Efficiency V and a Haste II beacon
		public decimal MiningTime { get; } = miningTime;
		public int MinimumDrops { get; } = minimumDrops;
		public int MaximumDrops { get; } = maximumDrops;
	}
}
