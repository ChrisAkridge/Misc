using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.CROBPackageCalc
{
	internal static class Simulator
	{
		private const int PackagePointsToFirstCoupon = 5;
		private const int PackagePointsToSecondCoupon = 35;
		private const int PackagePointsToThirdCoupon = 140;

		private static readonly List<PackageDetails> packages = GetPackages();
		private static readonly List<PackageDetails> firstCouponPackages = packages.Where(p => p.PretaxPrice <= 4.99m).ToList();
		private static readonly List<PackageDetails> secondCouponPackages = packages.Where(p => p.PretaxPrice <= 9.99m).ToList();

		private static List<PackageDetails> GetPackages() =>
		[
			new("Welcome Back! Package Vol.2", 6.99m, 7400, 7),
			new("Legendary Special Rainbow Cube Package x1", 23.99m, 25000, 33),
			new("Legendary Special Rainbow Cube Package x3", 23.99m * 3m, 70000, 33 * 3),
			new("Legendary Special Rainbow Cube Package x5", 23.99m * 5m, 115000, 33 * 5),
			new("Legendary Celebrations! Special Package", 41.99m, 28041, 55),
			new("Ananas Dragon's Rainbow Cube Package", 9.99m, 6000, 11),
			new("S9 Special! Welcome Package", 4.99m, 3000, 5),
			new("Season 9! Costume Bonus Package #2 x1", 4.99m, 1900 + 1500 + 1900, 5),
			new("Season 9! Costume Bonus Package #2 x3", 4.99m * 3, ((1900 + 1500) * 3) + 1900 + 1900, 5 * 3),
			new("Level 170 Special Package", 9.99m, 5000, 11),
			new("Limited! Dino Egg Costume Package", 9.99m, 3900, 11),
			new("Limited! Hot Pot Buckler Costume Package", 9.99m, 3900, 11),
			new("New Update Rainbow Cube Package", 9.99m, 6000, 11),
			new("Daily Rainbow Cubes Package", 0.99m, 400, 1),
			new("Breakout Package Ver.2", 79.99m, 25000, 110),
			new("Crystals & Rainbow Cubes Package", 23.99m, 7000, 33)
		];

		public static void Run()
		{
			// Why can't we just stack Season 9 Costume Bonuses over and over?
			for (var i = 0; i < 20; i++)
			{
				// Well, the reason is that we can only buy 3.
			}
			
			//for (decimal spendingLimit = 5.00m; spendingLimit < 100m; spendingLimit += 5.00m)
			//{
			//	Console.WriteLine($"Best tree for {spendingLimit:C}");
			//	var root = new PurchaseDecision();
			//	AddAllPossibleDecisions(root, spendingLimit, packages);
			
			//	var bestDecision = FindBestDecision(root);
			//	PrintDecisionTreeUpwards(bestDecision, 0);
			//}
		}

		private static void StackEm(PurchaseDecision decision)
		{
			
		}

		private static void AddAllPossibleDecisions(PurchaseDecision decision,
			decimal spendingLimit,
			List<PackageDetails> availablePackages,
			bool fromCoupon = false)
		{
			foreach (var availablePackage in availablePackages)
			{
				if (!fromCoupon)
				{
					if (decision.MoneySpentAtThisStep + availablePackage.PostTaxPrice > spendingLimit) { continue; }
				}

				var newDecision = decision.SetNextPurchaseIfBetter(availablePackage, fromCoupon);

				switch (decision.PackagePointsReceivedAtThisStep)
				{
					case < PackagePointsToThirdCoupon when newDecision.PackagePointsReceivedAtThisStep >= PackagePointsToThirdCoupon:
						AddAllPossibleDecisions(newDecision, spendingLimit, secondCouponPackages, true);

						break;
					case < PackagePointsToSecondCoupon when newDecision.PackagePointsReceivedAtThisStep >= PackagePointsToSecondCoupon:
						AddAllPossibleDecisions(newDecision, spendingLimit, firstCouponPackages, true);

						break;
					case < PackagePointsToFirstCoupon when newDecision.PackagePointsReceivedAtThisStep >= PackagePointsToFirstCoupon:
						AddAllPossibleDecisions(newDecision, spendingLimit, firstCouponPackages, true);

						break;
					default:
						AddAllPossibleDecisions(newDecision, spendingLimit, packages);

						break;
				}
			}
		}

		private static void PrintDecisionTree(PurchaseDecision decision, int depth)
		{
			while (true)
			{
				Console.WriteLine($"{new string(' ', depth * 2)}({decision.RainbowCubesPerDollar:F2} per dollar) Step #{depth}: {decision.PurchasedPackage?.PackageName ?? "(root)"}, spent {decision.MoneySpentAtThisStep:C}, got {decision.RainbowCubesReceivedAtThisStep} Cubes");

				if (decision.BestNextDecision != null)
				{
					decision = decision.BestNextDecision;
					depth += 1;

					continue;
				}

				break;
			}
		}

		private static void PrintDecisionTreeUpwards(PurchaseDecision decision, int depth)
		{
			while (true)
			{
				Console.WriteLine($"{new string(' ', depth * 2)}({decision.RainbowCubesPerDollar:F2} per dollar) Step #{depth}: {decision.PurchasedPackage?.PackageName ?? "(root)"}, spent {decision.MoneySpentAtThisStep:C}, got {decision.RainbowCubesReceivedAtThisStep} Cubes");

				if (decision.Parent != null)
				{
					decision = decision.Parent;
					depth += 1;

					continue;
				}

				break;
			}
		}

		private static PurchaseDecision FindBestDecision(PurchaseDecision decision)
		{
			var bestDecision = decision;

			while (true)
			{
				if (decision.BestNextDecision != null)
				{
					decision = decision.BestNextDecision;

					if (decision.RainbowCubesPerDollar > bestDecision.RainbowCubesPerDollar)
					{
						bestDecision = decision;
					}

					continue;
				}

				break;
			}
			
			return bestDecision;
		}
	}
}
