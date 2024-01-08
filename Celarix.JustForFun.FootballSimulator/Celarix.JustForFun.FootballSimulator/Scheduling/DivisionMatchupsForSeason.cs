using System;
using System.Collections.Generic;
using System.Linq;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal readonly struct DivisionMatchupsForSeason
{
    public Division IntraconferenceOpponentDivision { get; }
    public Division InterconferenceOpponentDivision { get; }
    public Division[] RemainingIntraconferenceOpponentDivisions { get; }

    public DivisionMatchupsForSeason(Division typeIIOpponentDivision, Division typeIIIOpponentDivision,
        Division firstTypeIVOpponentDivision, Division secondTypeIVOpponentDivision)
    {
        IntraconferenceOpponentDivision = typeIIOpponentDivision;
        InterconferenceOpponentDivision = typeIIIOpponentDivision;

        RemainingIntraconferenceOpponentDivisions = new[]
        {
            firstTypeIVOpponentDivision, secondTypeIVOpponentDivision
        };
    }
}