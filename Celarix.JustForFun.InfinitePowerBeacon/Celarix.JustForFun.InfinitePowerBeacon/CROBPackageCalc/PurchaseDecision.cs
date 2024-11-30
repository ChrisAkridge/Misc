using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.CROBPackageCalc
{
	internal class PurchaseDecision
	{
		public PurchaseDecision? Parent { get; set; }
		public PackageDetails? PurchasedPackage { get; set; }
		public decimal MoneySpentAtThisStep { get; set; }
		public int RainbowCubesReceivedAtThisStep { get; set; }
		public int PackagePointsReceivedAtThisStep { get; set; }
		public decimal RainbowCubesPerDollar =>
			MoneySpentAtThisStep != 0m
				? RainbowCubesReceivedAtThisStep / MoneySpentAtThisStep
				: 0m;
		public PurchaseDecision? BestNextDecision { get; private set; }

		public PurchaseDecision SetNextPurchaseIfBetter(PackageDetails package, bool fromCoupon = false)
		{
			var newMoneySpent = !fromCoupon
				? MoneySpentAtThisStep + package.PostTaxPrice
				: MoneySpentAtThisStep;
			var newCubesReceived = RainbowCubesReceivedAtThisStep + package.RainbowCubes;
			var newCubesPerDollar = newCubesReceived / newMoneySpent;

			if (BestNextDecision == null
			    || (newCubesPerDollar > BestNextDecision.RainbowCubesPerDollar
					&& newCubesPerDollar > RainbowCubesPerDollar))
			{
				BestNextDecision = new PurchaseDecision
				{
					Parent = this,
					PurchasedPackage = package,
					MoneySpentAtThisStep = !fromCoupon
						? MoneySpentAtThisStep + package.PostTaxPrice
						: MoneySpentAtThisStep,
					RainbowCubesReceivedAtThisStep = RainbowCubesReceivedAtThisStep + package.RainbowCubes,
					PackagePointsReceivedAtThisStep = PackagePointsReceivedAtThisStep + package.PackagePoints
				};
			}
			
			return BestNextDecision;
		}

		/// <summary>Returns a string that represents the current object.</summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString() => $"({RainbowCubesPerDollar:F2} cubes per dollar)";
	}
}
