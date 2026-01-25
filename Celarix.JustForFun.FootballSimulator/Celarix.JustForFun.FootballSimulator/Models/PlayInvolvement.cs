using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    //internal sealed class PlayInvolvement
    //{
    //    public bool InvolvesOffenseRun { get; set; }
    //    public bool InvolvesOffensePass { get; set; }
    //    public bool InvolvesKick { get; set; }
    //    public bool InvolvesDefenseRun { get; set; }
    //    public int OffensivePlayersInvolved { get; set; }
    //    public int DefensivePlayersInvolved { get; set; }
    //}

    public record PlayInvolvement(
        bool InvolvesOffenseRun,
        bool InvolvesOffensePass,
        bool InvolvesKick,
        bool InvolvesDefenseRun,
        int OffensivePlayersInvolved,
        int DefensivePlayersInvolved
    );
}
