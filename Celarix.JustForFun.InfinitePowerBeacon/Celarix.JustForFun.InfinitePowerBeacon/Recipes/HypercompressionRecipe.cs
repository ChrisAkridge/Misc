using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class HypercompressionRecipe(string ingredientReference, int ingredientCount)
		: Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Hypercompression;

		public string IngredientReference { get; } = ingredientReference;
		public int IngredientCount { get; } = ingredientCount;
	}
}
