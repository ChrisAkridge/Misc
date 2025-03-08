using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;

public readonly struct Meter
{
    private readonly double meters;

    public Meter(double meters) => this.meters = meters;

    public double Feet() => meters * 3.28084d;
}