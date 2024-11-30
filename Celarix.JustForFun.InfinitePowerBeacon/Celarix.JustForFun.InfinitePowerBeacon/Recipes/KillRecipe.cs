using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class KillRecipe(string mobReference, decimal dropRate) : Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Killing;

		public string MobReference { get; } = mobReference;
		public decimal DropRate { get; } = dropRate;
	}
}
