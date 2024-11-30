using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class OtherRecipe(string requirementReference) : Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Nothing;
		
		public string RequirementReference { get; } = requirementReference;
	}
}
