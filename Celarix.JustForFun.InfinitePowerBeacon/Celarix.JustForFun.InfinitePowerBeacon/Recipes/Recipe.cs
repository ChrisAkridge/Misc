using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.Recipes
{
	internal abstract class Recipe
	{
		public abstract RecipeCostType CostType { get; }
	}
}
