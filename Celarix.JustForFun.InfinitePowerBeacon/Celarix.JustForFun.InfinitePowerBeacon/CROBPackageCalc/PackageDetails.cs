using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.CROBPackageCalc
{
	internal sealed class PackageDetails
	{
		public string PackageName { get; set; }
		public decimal PretaxPrice { get; set; }
		public decimal PostTaxPrice => Math.Round(PretaxPrice * 1.06m, 2);
		public int RainbowCubes { get; set; }
		public decimal RainbowCubesPerDollar => RainbowCubes / PostTaxPrice;
		// This is for the game Cookie Run: OvenBreak, by the way
		public int PackagePoints { get; set; }

		public PackageDetails(string packageName, decimal pretaxPrice, int rainbowCubes, int packagePoints)
		{
			PackageName = packageName;
			PretaxPrice = pretaxPrice;
			RainbowCubes = rainbowCubes;
			PackagePoints = packagePoints;
		}
		
		public override string ToString() => $"{PackageName} +{RainbowCubes} ({PostTaxPrice:C})";
	}
}
