using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.InfinitePowerBeacon.BetterCROBPackageCalc
{
    internal static class BetterSimulator
    {
        // Here's the problem domain:
        // We have a list of packages we can buy, each with a price, a number of Rainbow Cubes, and a number of Package Points.
        // We want Rainbow Cubes. When we accumulate 5 Package Points, we can buy a package worth $4.99 for free.
        // At 35 Package Points, we can buy another package worth $4.99 for free.
        // And, finally, at 140 Package Points, we can buy a package worth $9.99 for free.

        // We can represent every chain of purchases as a giant tree.
        // Each node in the tree represents a purchase decision.
        // At each node, we've spent a certain amount of money and gotten a certain number of Cubes.
        // Therefore, each node has a Cubes per dollar value. Our goal is to find the highest Cubes per dollar value in the tree.

        // There are other complications. Some packages can only be bought a certain number of times.
        // And some packages have multi-purchase bonuses.

        // The tree is limited in height. We set a spending limit, and we can't go over it.
        // But this tree is huge. We can't possibly store every path in memory.
        // But we may be able to deal with the tree implicitly.

        // Each node of the idealized tree needs the following:
        // - Which package we bought
        // - How much money we've spent in total (getting the free packages doesn't add to this)
        // - How many Rainbow Cubes we've gotten in total, which also lets us calculate the Cubes per dollar value
        //	(also, any multi-purchase bonuses we've gotten will be added here)
        // - How many Package Points we have now
        // - How many more times we can buy each package

        // But we don't want to generate the whole tree, just search it. Let's list all our packages.
        // #1: Welcome Back! Package Vol.2: 7,400 Rainbow Cubes, $6.99, 7 Package Points, limit 1
        // #2: Legendary Special Rainbow Cube Package: 20,000 Rainbow Cubes, $23.99, 33 Package Points, limit 5
        //	(1x bonus: 5,000 Rainbow Cubes)
        //  (3x bonus: 5,000 Rainbow Cubes)
        //	(5x bonus: 5,000 Rainbow Cubes)
        // #3: Legendary Celebrations! Special Package: 20,000 Rainbow Cubes, $41.99, 55 Package Points, limit 1
        // #4: Ananas Dragon's Rainbow Cube Package: 6,000 Rainbow Cubes, $9.99, 11 Package Points, limit 5
        //	This one's kinda weird. There is another package offering only Crystals. If we buy one of that, we can reach a 6x bonus,
        //	which otherwise wouldn't be reachable... Okay, so what we'll do is include the other package as having 0 Rainbow Cubes,
        //	and we'll have a function that checks for any bonuses given the existing path.
        // #5: Pitaya Dragon's Crystal Package: 0 Rainbow Cubes, $9.99, 11 Package Points, limit 5
        //	Now we can do the bonuses, shared between the two:
        //	(1x bonus: 3,000 Rainbow Cubes)
        //	(4x bonus: 1,500 Rainbow Cubes)
        //	(6x bonus: 3,000 Rainbow Cubes)
        // #6: S9 Special! Welcome Package: 3,000 Rainbow Cubes, $4.99, 5 Package Points, limit 1
        // #7: Season 9! Costume Bonus Package #2: 3,400 Rainbow Cubes, $4.99, 5 Package Points, limit 3
        //	(1x bonus: 1,900 Rainbow Cubes)
        //	(3x bonus: 1,900 Rainbow Cubes)
        // #8: Level 170 Special Package: 5,000 Rainbow Cubes, $9.99, 11 Package Points, limit 1
        // #9: Limited! Dino Egg Costume Package: 3,900 Rainbow Cubes, $9.99, 11 Package Points, limit 1
        // #10: Limited! Hot Pot Buckler Costume Package: 3,900 Rainbow Cubes, $9.99, 11 Package Points, limit 1
        // #11: New Update Rainbow Cube Package: 6,000 Rainbow Cubes, $9.99, 11 Package Points, limit 5
        // #12: Daily Rainbow Cube Package: 400 Rainbow Cubes, $0.99, 1 Package Point, limit 1
        // #13: Breakout Package Ver.2: 25,000 Rainbow Cubes, $79.99, 110 Package Points, limit 10
        // #14: Crystals & Rainbow Cubes Package: 7,000 Rainbow Cubes, $23.99, 33 Package Points, no limit
        public static List<Package> GetPackages() =>
        [
			new("Welcome Back! Package Vol.2", 7400, 6.99m, 7, 1),
			new("Legendary Special Rainbow Cube Package", 20000, 23.99m, 33, 5),
			new("Legendary Celebrations! Special Package", 20000, 41.99m, 55, 1),
			new("Ananas Dragon's Rainbow Cube Package", 6000, 9.99m, 11, 5),
			new("Pitaya Dragon's Crystal Package", 0, 9.99m, 11, 5),
			new("S9 Special! Welcome Package", 3000, 4.99m, 5, 1),
			new("Season 9! Costume Bonus Package #2", 3400, 4.99m, 5, 3),
			new("Level 170 Special Package", 5000, 9.99m, 11, 1),
			new("Limited! Dino Egg Costume Package", 3900, 9.99m, 11, 1),
			new("Limited! Hot Pot Buckler Costume Package", 3900, 9.99m, 11, 1),
			new("New Update Rainbow Cube Package", 6000, 9.99m, 11, 5),
			new("Daily Rainbow Cube Package", 400, 0.99m, 1, 1),
			new("Breakout Package Ver.2", 25000, 79.99m, 110, 10),
			new("Crystals & Rainbow Cubes Package", 7000, 23.99m, 33)
        ];

        // This tree is really tough to reason about. It's way too big to store in memory,
        // and would take way too long to walk even if we kept it implicit. But if we decide to use a greedy
        // algorithm that picks the best next package, we get stuck in local maxima. I'm sure there's some
        // research on finding a global maximum in a computationally intractable tree. I'll have to look it up.

        // Simulated annealing looks like a good candidate. We can start with a random path, then make random changes.
        // Here's the basic idea. The tree is represented as a state machine "visiting" a node. The machine
        // keeps track of the money spent, Cubes and Package Points received, and the number of times each package
        // has been bought. It can "see" all the next available options, and it knows which ones are impossible
        // due to hitting the limit, and it also knows which ones are impossible due to not having enough money.
        // It computes bonuses as well. The path itself represents the purchase history. To represent a random
        // walk, we can just pick options randomly until we hit the spending limit. Then we can use simulated
        // annealing to make random changes to the path. We can use the Cubes per dollar value as the "energy"
        // of the system, so when the temperature is high, we will jump much more freely around random options,
        // but we'll choose options closer to the current one when the temperature is low.

        // A path is just represented as a collection of package indices, 0 through 13. Given the more
        // complete list, we can then rebuild the packages purchased and the Cubes received. How we'll do
        // the annealing is that we will start by randomly changing every single package in the path. Then,
        // we'll discretely move to changing all but the first package, then all but the first two, and so on.
        // Additionally, each random change in a single step will be between a smaller and smaller range
        // of Cubes per dollar, and each stage of annealing will last for a shorter number of steps.
        public static void Run()
        {
	        // Best so far: $10.00
	        var packages = GetPackages();
	        var spendingLimit = 100.00m;
	        var longestPathLengthLastRound = 0;
	        var bestCubesPerDollarSoFar = decimal.MinValue;
	        IReadOnlyList<int>? bestPathSoFar = null;
	        var consoleStream = new StreamWriter(Console.OpenStandardOutput());
	        
			for (var temperature = 1d; temperature > 0.00d; temperature -= 0.05d)
	        {
		        // So we'll do 20 temperatures from 1 to 0.05. To calculate the number of
		        // runs, we'll compute 10^(5 * temperature) to start at a hundred thousand runs.
		        var runs = (int)Math.Pow(10, 5 * temperature);
		        
		        // Then, we'll pick a number of steps to fix, which is just the inverse
		        // of the temperature.
		        var fixedStepProportion = 1d - temperature;
		        var fixedStepCount = (int)(longestPathLengthLastRound * fixedStepProportion);
		        
		        // Next, we'll start a single full path as a seed.
		        IReadOnlyList<int> seedPath;

		        if (bestPathSoFar == null)
		        {
			        var seedPathVisitor = PathVisitor.Create(packages, temperature, spendingLimit);
			        seedPath = GetFinalPathOfVisitor(seedPathVisitor);
			        bestPathSoFar = seedPath;
			        bestCubesPerDollarSoFar = seedPathVisitor.CurrentRainbowCubesPerDollar;
		        }
		        else
		        {
			        seedPath = bestPathSoFar;
		        }
		        var fixedSteps = seedPath.Take(fixedStepCount).ToArray();
		        
		        // Finally, we'll run the annealing.
		        for (var run = 0; run < runs; run++)
		        {
			        var pathVisitor = PathVisitor.Create(packages, temperature, spendingLimit, fixedSteps);
			        var finalPath = GetFinalPathOfVisitor(pathVisitor);

			        if (finalPath.Count > longestPathLengthLastRound) { longestPathLengthLastRound = finalPath.Count; }

			        if (pathVisitor.CurrentRainbowCubesPerDollar > bestCubesPerDollarSoFar)
			        {
				        bestCubesPerDollarSoFar = pathVisitor.CurrentRainbowCubesPerDollar;
				        bestPathSoFar = finalPath;
			        }
		        }
		        
		        // Print the best path to the console.
		        PrintPathToStream(packages, bestPathSoFar!, bestCubesPerDollarSoFar, temperature, consoleStream);
		        consoleStream.Flush();
	        }
        }

        private static IReadOnlyList<int> GetFinalPathOfVisitor(PathVisitor visitor)
        {
	        while (visitor.TryVisitRandomPackage()) { }

	        return visitor.VisitedPackageIndices;
        }

        private static void PrintPathToStream(List<Package> packages, IReadOnlyList<int> purchaseSequence,
	        decimal pathRainbowCubesPerDollar, double temperature, StreamWriter writer)
        {
	        writer.WriteLine($"({pathRainbowCubesPerDollar:F2} Rainbow Cubes per Dollar, temperature {temperature:F2})");
	        var totalCubes = 0m;
	        var totalSpent = 0m;
	        
	        for (var i = 0; i < purchaseSequence.Count; i++)
	        {
		        var packageIndex = purchaseSequence[i];
		        var package = packages[packageIndex];
		        totalCubes += package.RainbowCubes;
		        totalSpent += package.PostTaxPrice;
		        writer.WriteLine($"\tStep #{i + 1}: Buy \"{package.Name}\" for {package.PostTaxPrice:C} (coupons might apply), get {package.RainbowCubes} Cubes");
	        }
	        
	        writer.WriteLine($"\tTotal spent: {totalSpent:C}, total Cubes: {totalCubes}");
        }
    }
}
