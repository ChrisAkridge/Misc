using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Random
{
    public interface IRandom
    {
        public bool Chance(double chance);
        public T Choice<T>(IReadOnlyList<T> items);
        public double NextDouble();
        public double SampleNormalDistribution(double mean, double stddev);
        public int Next(int maxValue);
        public int Next(int minValue, int maxValue);
        public void Shuffle<T>(IList<T> list);
        public IReadOnlyList<T> ShuffleIntoNewList<T>(IReadOnlyList<T> list);
    }
}
