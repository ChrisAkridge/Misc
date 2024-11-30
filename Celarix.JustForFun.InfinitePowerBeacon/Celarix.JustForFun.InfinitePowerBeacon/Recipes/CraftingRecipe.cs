using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class CraftingRecipe(int resultCount, params string[] ingredientReferences)
		: Recipe
	{
		private readonly string[] ingredientReferences = ingredientReferences;
		
		public IReadOnlyList<string> IngredientReferences => ingredientReferences;
		public int ResultCount { get; } = resultCount;

		public override RecipeCostType CostType => RecipeCostType.Nothing;
	}
}
