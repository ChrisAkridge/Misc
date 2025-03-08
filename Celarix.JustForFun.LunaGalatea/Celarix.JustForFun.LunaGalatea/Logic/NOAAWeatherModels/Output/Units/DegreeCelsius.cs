using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units
{
    public readonly struct DegreeCelsius
    {
        private readonly double degrees;

        public DegreeCelsius(double degrees) => this.degrees = degrees;

        public double Fahrenheit() => ((degrees * 9) / 5) + 32;
    }
}
