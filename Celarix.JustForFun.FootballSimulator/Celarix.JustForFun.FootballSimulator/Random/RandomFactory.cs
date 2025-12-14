using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Random
{
    public sealed class RandomFactory : IRandomFactory
    {
        public IRandom Create(int seed)
        {
            return new RandomWrapper(seed);
        }

        public IRandom Create()
        {
            return new RandomWrapper();
        }
    }
}
