using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class SmeltingRecipe(string ingredientReference) : Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Smelting;
		
		public string IngredientReference { get; } = ingredientReference;
	}
}
