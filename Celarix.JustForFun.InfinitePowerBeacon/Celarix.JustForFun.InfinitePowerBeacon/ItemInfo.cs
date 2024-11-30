using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.InfinitePowerBeacon.Recipes;

namespace Celarix.JustForFun.InfinitePowerBeacon
{
	internal sealed class ItemInfo(string name, Recipe madeBy)
	{
		public string Name { get; } = name;
		public Recipe MadeBy { get; } = madeBy;
	}
}
