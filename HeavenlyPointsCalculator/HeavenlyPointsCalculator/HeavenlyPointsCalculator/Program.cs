using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenlyPointsCalculator
{
	class Program
	{
		private const double PriceIncreaseValue = 1.01d;

		static void Main(string[] args)
		{
			Console.WriteLine("Meijer Statistics: Heavenly Points Calculator");
			Console.WriteLine();
			Console.Write("Number of total points: ");
			int totalPoints = int.Parse(Console.ReadLine());
			Console.Write("Number of heavenly points already: ");
			int heavenlyPointsAlready = int.Parse(Console.ReadLine());

			int newHeavenlyPoints = CalculateHeavenlyPoints(totalPoints, heavenlyPointsAlready);
			int pointsToNextHeavenlyPoint = PointsToNextHeavenlyPoint(totalPoints, heavenlyPointsAlready);
			Console.WriteLine("Number of Heavenly Points gained by resetting: {0} (+{1}% multiplier)", newHeavenlyPoints, newHeavenlyPoints * 2);
			//Console.WriteLine("Number of points to next Heavenly Point: {0}", pointsToNextHeavenlyPoint);
			Console.Read();
		}

		private static int CalculateHeavenlyPoints(int totalPoints, int heavenlyPointsAlready)
		{
			double heavenlyPointCost = 100d;
			while (heavenlyPointsAlready > 0)
			{
				heavenlyPointCost *= PriceIncreaseValue;
				heavenlyPointsAlready--;
			}

			int newHeavenlyPoints = 0;

			while (totalPoints > 0)
			{
				totalPoints -= (int)Math.Ceiling(heavenlyPointCost);
				heavenlyPointCost *= PriceIncreaseValue;
				newHeavenlyPoints++;
			}

			return newHeavenlyPoints;
		}

		private static int PointsToNextHeavenlyPoint(int totalPoints, int heavenlyPointsAlready)
		{
			double heavenlyPointsCost = 100d;
			int pointsSpent = 0;
			while (heavenlyPointsAlready > 0)
			{
				pointsSpent += (int)Math.Ceiling(heavenlyPointsCost);
				heavenlyPointsCost *= PriceIncreaseValue;
				heavenlyPointsAlready--;
			}
			totalPoints -= pointsSpent;
			return (int)Math.Ceiling(heavenlyPointsCost) - totalPoints;
		}
	}
}
