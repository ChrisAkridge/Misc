using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;

public readonly struct Pascal
{
    private readonly double pascals;

    public Pascal(double pascals) => this.pascals = pascals;

    public double MillimetersOfMercury() => pascals / 133.3d;
}