using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.BetterCROBPackageCalc
{
	internal sealed class PathVisitor
	{
		private const decimal LowCouponValue = 4.99m;
		private const decimal HighCouponValue = 9.99m;
		
		private static readonly Random random = new Random();

		private Dictionary<int, Package> packagesByIndex;
		private readonly List<int> visitedPackageIndices = [];
		private readonly int[] packagePurchaseCount = Enumerable.Repeat(0, 14).ToArray();
		private readonly CouponAvailability[] couponAvailable = [CouponAvailability.NotYetEarned, CouponAvailability.NotYetEarned, CouponAvailability.NotYetEarned];
		private decimal totalSpent = 0m;
		private int totalRainbowCubes = 0;
		private int totalPackagePoints = 0;
		private decimal spendingLimit;
		private double temperature;
		
		public decimal CurrentRainbowCubesPerDollar => totalSpent != 0m ? totalRainbowCubes / totalSpent : 0m;
		
		public IReadOnlyList<int> VisitedPackageIndices => visitedPackageIndices;

		public static PathVisitor Create(List<Package> packages, double temperature, decimal spendingLimit,
			IEnumerable<int>? visitedPackageIndices = null)
		{			
			var visitor = new PathVisitor
			{
				packagesByIndex = packages.Select((p, i) => new
				{
					Package = p, Index = i
				})
				.ToDictionary(t => t.Index, t => t.Package),
				temperature = temperature,
				spendingLimit = spendingLimit
			};

			if (visitedPackageIndices != null)
			{
				foreach (var visitedPackageIndex in visitedPackageIndices)
				{
					visitor.VisitSpecificPackage(visitedPackageIndex);
				}
			}

			return visitor;
		}

		private void VisitSpecificPackage(int packageIndex)
		{
			visitedPackageIndices.Add(packageIndex);
			var package = packagesByIndex[packageIndex];
			
			// See if we can use a coupon here.
			if (couponAvailable[0] == CouponAvailability.Earned && package.PretaxPrice == LowCouponValue)
			{
				couponAvailable[0] = CouponAvailability.Used;
			}
			else if (couponAvailable[1] == CouponAvailability.Earned && package.PretaxPrice == LowCouponValue)
			{
				couponAvailable[1] = CouponAvailability.Used;
			}
			else if (couponAvailable[2] == CouponAvailability.Earned && package.PretaxPrice == HighCouponValue)
			{
				couponAvailable[2] = CouponAvailability.Used;
			}
			else { totalSpent += package.PostTaxPrice; }
			
			totalRainbowCubes += package.RainbowCubes + GetBonusRainbowCubesFromPackagePurchase(packageIndex);
			totalPackagePoints += package.PackagePoints;
			packagePurchaseCount[packageIndex] += 1;
			UnlockCouponsIfAvailable();
		}

		public bool TryVisitRandomPackage()
		{
			// Use a coupon immediately if possible.
			var earnedCouponIndex = Array.IndexOf(couponAvailable, CouponAvailability.Earned);
			IEnumerable<KeyValuePair<int, Package>> availablePackageQuery;

			if (earnedCouponIndex != -1)
			{
				availablePackageQuery = packagesByIndex.Where(p => p.Value.PretaxPrice
					== earnedCouponIndex switch
					{
						0 or 1 => LowCouponValue,
						2 => HighCouponValue,
						_ => throw new InvalidOperationException("Invalid coupon index.")
					});
			}
			else
			{
				// Filter our choices to packages that we can still afford
				// and that haven't hit their purchase limit.
				availablePackageQuery = packagesByIndex
					.Where(kvp => kvp.Value.PostTaxPrice + totalSpent <= spendingLimit
						&& packagePurchaseCount[kvp.Key] < kvp.Value.PurchaseLimit);
			}
			
			var availablePackages = availablePackageQuery.ToList();

			if (availablePackages.Count == 0) { return false; }
			
			VisitSpecificPackage(GetRandomPackageIndexByTemperature(availablePackages));
			return true;
		}

		private void UnlockCouponsIfAvailable()
		{
			if (totalPackagePoints >= 5 && couponAvailable[0] == CouponAvailability.NotYetEarned)
			{
				couponAvailable[0] = CouponAvailability.Earned;
			}

			if (totalPackagePoints >= 35 && couponAvailable[1] == CouponAvailability.NotYetEarned)
			{
				couponAvailable[1] = CouponAvailability.Earned;
			}

			if (totalPackagePoints >= 140 && couponAvailable[2] == CouponAvailability.NotYetEarned)
			{
				couponAvailable[2] = CouponAvailability.Earned;
			}
		}

		private int GetBonusRainbowCubesFromPackagePurchase(int packageIndex)
		{
			switch (packageIndex)
			{
				case 1:
					switch (packagePurchaseCount[1] + 1)
					{
						// Hardcoded indices. Yay.
						case 1:
						case 3:
						case 5:
							return 5000;
					}

					break;
				case 3 or 4:
				{
					var combinedPackagePurchases = packagePurchaseCount[3] + packagePurchaseCount[4] + 1;

					switch (combinedPackagePurchases)
					{
						case 1:
							return 3000;
						case 4:
							return 1500;
						case 6:
							return 3000;
					}

					break;
				}
				case 6:
					switch (packagePurchaseCount[6] + 1)
					{
						case 1:
						case 3:
							return 1900;
					}

					break;
			}

			return 0;
		}

		private static bool IsValidForCoupon(int packageIndex, CouponLevel couponLevel) =>
			couponLevel switch
			{
				CouponLevel.High => packageIndex is 3 or 4 or 7 or 8 or 9 or 10,
				CouponLevel.Low => packageIndex is 5 or 6,
				CouponLevel.NoCoupon => true,
				_ => throw new ArgumentException("Invalid coupon level.", nameof(couponLevel))
			};

		private int GetRandomPackageIndexByTemperature(IReadOnlyList<KeyValuePair<int, Package>> availablePackages)
		{
			// First, select out the packages and their indices.
			var eligiblePackages = availablePackages
				.Select(kvp => new
				{
					Package = kvp.Value,
					OriginalIndex = kvp.Key,
					CubesPerDollarWithBonuses = (kvp.Value.RainbowCubes + GetBonusRainbowCubesFromPackagePurchase(kvp.Key)) / kvp.Value.PostTaxPrice
				})
				.ToList();
			
			// Then, compute their Cubes per dollar value and sort descending.
			eligiblePackages.Sort((a, b) =>
			{
				var aCubesPerDollar = a.CubesPerDollarWithBonuses;
				var bCubesPerDollar = b.CubesPerDollarWithBonuses;

				return bCubesPerDollar.CompareTo(aCubesPerDollar);
			});
			
			// The temperature is between 0 and 1. At 1, we are free to pick any package, but at lower
			// temperatures, we should pick one closer to where we are. At, for example, 0.5, we are
			// limited to only the half of packages around where we are now.

			var highestSortedIndexGreaterThanUs = 0;

			for (var i = 0; i < eligiblePackages.Count; i++)
			{
				if (eligiblePackages[i].CubesPerDollarWithBonuses < CurrentRainbowCubesPerDollar)
				{
					highestSortedIndexGreaterThanUs = i;

					break;
				}
			}
			
			var accessiblePackageCount = availablePackages.Count * temperature;
			var minIndex = (int)Math.Clamp(highestSortedIndexGreaterThanUs - accessiblePackageCount, 0, availablePackages.Count - 1);
			var maxIndex = (int)Math.Clamp(highestSortedIndexGreaterThanUs + accessiblePackageCount, 0, availablePackages.Count - 1);
			var randomIndex = random.Next(minIndex, maxIndex);
			return eligiblePackages[randomIndex].OriginalIndex;
		}
	}
}
