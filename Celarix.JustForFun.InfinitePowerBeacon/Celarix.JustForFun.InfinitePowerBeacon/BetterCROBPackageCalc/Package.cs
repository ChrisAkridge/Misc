using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.BetterCROBPackageCalc
{
	internal sealed class Package(string name, int rainbowCubes, decimal pretaxPrice, int packagePoints, int? purchaseLimit = null)
	{
		public string Name { get; } = name;
		public decimal PretaxPrice { get; } = pretaxPrice;
		public decimal PostTaxPrice => Math.Round(PretaxPrice * 1.06m, 2);
		public int RainbowCubes { get; } = rainbowCubes;
		public decimal RainbowCubesPerDollar => RainbowCubes / PostTaxPrice;
		public int PackagePoints { get; } = packagePoints;
		public int? PurchaseLimit { get; } = purchaseLimit;
	}
}
