using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Random
{
    internal sealed class RandomWrapper : IRandom
    {
        private readonly System.Random random;

        public RandomWrapper(int? seed = null)
        {
            random = seed.HasValue
                ? new System.Random(seed.Value)
                : new System.Random();
        }

        public bool Chance(double chance) => random.Chance(chance);

        public T Choice<T>(IReadOnlyList<T> items)
        {
            if (items == null || items.Count == 0)
            {
                throw new ArgumentException("The collection must contain at least one item.", nameof(items));
            }
            int index = random.Next(items.Count);
            return items[index];
        }

        public int Next(int maxValue) => random.Next(maxValue);
        public int Next(int minValue, int maxValue) => random.Next(minValue, maxValue);
        public double NextDouble() => random.NextDouble();
        public double SampleNormalDistribution(double mean, double stddev) => random.SampleNormalDistribution(mean, stddev);

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        public IReadOnlyList<T> ShuffleIntoNewList<T>(IReadOnlyList<T> list)
        {
            var shuffledList = new List<T>(list);
            Shuffle(shuffledList);
            return shuffledList;
        }
    }
}
