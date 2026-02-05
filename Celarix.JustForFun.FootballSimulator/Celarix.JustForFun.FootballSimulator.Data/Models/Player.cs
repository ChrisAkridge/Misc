using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
	public class Player
	{
		[Key]
		public int PlayerID { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTimeOffset DateOfBirth { get; set; }
		public bool Retired { get; set; }
		public bool UndraftedFreeAgent { get; set; }

        public List<PlayerRosterPosition> RosterPositions { get; set; }
		
		public int CareerWins => RosterPositions.Sum(rp => rp.TeamWins);
		public int CareerLosses => RosterPositions.Sum(rp => rp.TeamLosses);
		public int CareerTies => RosterPositions.Sum(rp => rp.TeamTies);
		
		public int CareerPassingYards => RosterPositions.Sum(rp => rp.TeamPassingYards);
		public int CareerInterceptions => RosterPositions.Sum(rp => rp.TeamInterceptions);
		public int CareerPassAttempts => RosterPositions.Sum(rp => rp.TeamPassAttempts);
		public int CareerPassCompletions => RosterPositions.Sum(rp => rp.TeamPassCompletions);
		
		public int CareerRushingAttempts => RosterPositions.Sum(rp => rp.TeamRushingAttempts);
		public int CareerRushingYards => RosterPositions.Sum(rp => rp.TeamRushingYards);
		public int CareerReceivingYards => RosterPositions.Sum(rp => rp.TeamReceivingYards);
		
		public int CareerTackles => RosterPositions.Sum(rp => rp.TeamTackles);
		public int CareerSacks => RosterPositions.Sum(rp => rp.TeamSacks);
		
		public int CareerKickReturns => RosterPositions.Sum(rp => rp.TeamKickReturns);
		public int CareerReturnYards => RosterPositions.Sum(rp => rp.TeamReturnYards);
		
		public int CareerKickoffs => RosterPositions.Sum(rp => rp.TeamKickoffs);
		public int CareerTouchbacks => RosterPositions.Sum(rp => rp.TeamTouchbacks);
		
		public int CareerTouchdownsScored => RosterPositions.Sum(rp => rp.TeamTouchdownsScored);
		public int CareerFieldGoalAttempts => RosterPositions.Sum(rp => rp.TeamFieldGoalAttempts);
		public int CareerFieldGoalsMade => RosterPositions.Sum(rp => rp.TeamFieldGoalsMade);
		public int CareerExtraPointAttempts => RosterPositions.Sum(rp => rp.TeamExtraPointAttempts);
		public int CareerExtraPointsMade => RosterPositions.Sum(rp => rp.TeamExtraPointsMade);
		public int CareerLongFieldGoal => RosterPositions.Max(rp => rp.TeamLongFieldGoal);
		
		public float CareerWinLossTieRecord => (CareerWins + (CareerTies / 2f)) / (CareerWins + CareerTies + CareerLosses);
		public float CareerPassCompletionPercentage => CareerPassCompletions / (float)CareerPassAttempts;
		public float CareerRushingYardsPerAttempt => CareerRushingYards / (float)CareerRushingAttempts;
		public float CareerKickReturnYardsPerAttempt => CareerReturnYards / (float)CareerKickReturns;
		public float CareerFieldGoalPercentage => CareerFieldGoalsMade / (float)CareerFieldGoalAttempts;
		public float CareerExtraPointPercentage => CareerExtraPointsMade / (float)CareerExtraPointAttempts;
		public int CareerTotalPoints => (CareerTouchdownsScored * 6) + (CareerFieldGoalsMade * 3) + CareerExtraPointsMade;

		public string FullName => $"{FirstName} {LastName}";
    }
}
