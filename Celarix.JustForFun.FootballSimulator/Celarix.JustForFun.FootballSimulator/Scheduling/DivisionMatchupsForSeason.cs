using System;
using System.Collections.Generic;
using System.Linq;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator.Scheduling;

internal readonly struct DivisionMatchupsForSeason(Division typeIIOpponentDivision, Division typeIIIOpponentDivision,
    Division firstTypeIVOpponentDivision, Division secondTypeIVOpponentDivision)
{
    public Division IntraconferenceOpponentDivision { get; } = typeIIOpponentDivision;
    public Division InterconferenceOpponentDivision { get; } = typeIIIOpponentDivision;
    public Division[] RemainingIntraconferenceOpponentDivisions { get; } = new[]
        {
            firstTypeIVOpponentDivision, secondTypeIVOpponentDivision
        };
}