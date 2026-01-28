using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    public record PlayInvolvement(
        bool InvolvesOffenseRun,
        bool InvolvesOffensePass,
        bool InvolvesKick,
        bool InvolvesDefenseRun,
        int OffensivePlayersInvolved,
        int DefensivePlayersInvolved
    );
}
