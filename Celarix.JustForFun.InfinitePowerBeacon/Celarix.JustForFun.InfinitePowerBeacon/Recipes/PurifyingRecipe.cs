using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class PurifyingRecipe(
		string overworldItemReference,
		string netherItemReference,
		string endItemReference,
		string itemToPurifyReference)
		: Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Nothing;
		
		public string OverworldItemReference { get; } = overworldItemReference;
		public string NetherItemReference { get; } = netherItemReference;
		public string EndItemReference { get; } = endItemReference;
		public string ItemToPurifyReference { get; } = itemToPurifyReference;
	}
}
