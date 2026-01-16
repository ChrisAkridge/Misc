using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Standings
{
    public sealed class TieGroup : IComparable<TieGroup>
    {
        private readonly List<TeamStats> tiedTeams;

        public int Position { get; }
        public IReadOnlyList<TeamStats> TiedTeams => tiedTeams.AsReadOnly();

        public TieGroup(int position)
        {
            Position = position;
            tiedTeams = [];
        }

        public int CompareTo(TieGroup? other)
        {
            if (other is null) return 1;
            return Position.CompareTo(other.Position);
        }
    }
}
