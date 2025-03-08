using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.NOAAWeatherModels.Output.Units;

public readonly struct KilometerPerHour
{
    private readonly double kilometerPerHour;

    public KilometerPerHour(double kilometerPerHour) =>
        this.kilometerPerHour = kilometerPerHour;

    public double MilesPerHour() => kilometerPerHour / 1.609d;
}