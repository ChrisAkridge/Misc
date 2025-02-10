using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;

public readonly struct Millimeter
{
    private readonly double millimeters;

    public Millimeter(double millimeters) => this.millimeters = millimeters;

    public double Inches() => millimeters / 25.4d;
}