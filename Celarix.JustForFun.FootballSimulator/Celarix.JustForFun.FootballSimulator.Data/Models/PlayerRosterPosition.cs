using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
	public class PlayerRosterPosition
	{
		public int PlayerID { get; set; }
		public Player Player { get; set; }
		public int TeamID { get; set; }
		public Team Team { get; set; }
		
		public bool CurrentPlayer { get; set; }
		public int JerseyNumber { get; set; }
		public BasicPlayerPosition Position { get; set; }

		public int TeamWins { get; set; }
		public int TeamLosses { get; set; }
		public int TeamTies { get; set; }

		public int TeamPassingYards { get; set; }
		public int TeamInterceptions { get; set; }
		public int TeamPassAttempts { get; set; }
		public int TeamPassCompletions { get; set; }

		public int TeamRushingAttempts { get; set; }
		public int TeamRushingYards { get; set; }
		public int TeamReceivingYards { get; set; }

		public int TeamTackles { get; set; }
		public int TeamSacks { get; set; }

		public int TeamKickReturns { get; set; }
		public int TeamReturnYards { get; set; }

		public int TeamKickoffs { get; set; }
		public int TeamTouchbacks { get; set; }

		public int TeamTouchdownsScored { get; set; }
		public int TeamFieldGoalAttempts { get; set; }
		public int TeamFieldGoalsMade { get; set; }
		public int TeamExtraPointAttempts { get; set; }
		public int TeamExtraPointsMade { get; set; }
		public int TeamLongFieldGoal { get; set; }
		
		public float WinLossTieRecord => (TeamWins + (TeamTies / 2f)) / (TeamWins + TeamTies + TeamLosses);
		public float PassCompletionPercentage => TeamPassCompletions / (float)TeamPassAttempts;
		public float RushingYardsPerAttempt => TeamRushingYards / (float)TeamRushingAttempts;
		public float KickReturnYardsPerAttempt => TeamReturnYards / (float)TeamKickReturns;
		public float FieldGoalPercentage => TeamFieldGoalsMade / (float)TeamFieldGoalAttempts;
		public float ExtraPointPercentage => TeamExtraPointsMade / (float)TeamExtraPointAttempts;
		public int TotalTeamPoints => (TeamTouchdownsScored * 6) + (TeamFieldGoalsMade * 3) + TeamExtraPointsMade;
	}
}
