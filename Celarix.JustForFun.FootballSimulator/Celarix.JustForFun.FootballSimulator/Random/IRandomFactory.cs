using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Random
{
    public interface IRandomFactory
    {
        public IRandom Create(int seed);
        public IRandom Create();
    }
}
