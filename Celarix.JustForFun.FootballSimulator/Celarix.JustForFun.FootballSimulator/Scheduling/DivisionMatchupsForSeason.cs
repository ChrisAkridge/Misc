using System;
using System.Collections.Generic;
using System.Linq;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal readonly struct DivisionMatchupsForSeason
{
    public Division TypeIIOpponentDivision { get; }
    public Division TypeIIIOpponentDivision { get; }
    public Division[] TypeIVOpponentDivisions { get; }

    public DivisionMatchupsForSeason(Division typeIIOpponentDivision, Division typeIIIOpponentDivision,
        Division firstTypeIVOpponentDivision, Division secondTypeIVOpponentDivision)
    {
        TypeIIOpponentDivision = typeIIOpponentDivision;
        TypeIIIOpponentDivision = typeIIIOpponentDivision;

        TypeIVOpponentDivisions = new[]
        {
            firstTypeIVOpponentDivision, secondTypeIVOpponentDivision
        };
    }
}