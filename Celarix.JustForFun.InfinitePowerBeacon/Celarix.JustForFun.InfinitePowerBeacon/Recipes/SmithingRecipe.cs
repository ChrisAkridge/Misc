using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal sealed class SmithingRecipe(
		string baseItemReference,
		string additiveItemReference,
		string templateReference)
		: Recipe
	{
		public override RecipeCostType CostType => RecipeCostType.Nothing;
		
		public string BaseItemReference { get; } = baseItemReference;
		public string AdditiveItemReference { get; } = additiveItemReference;
		public string TemplateReference { get; } = templateReference;
	}
}
