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
		public required string FirstName { get; set; }
		public required string LastName { get; set; }
		public DateTimeOffset DateOfBirth { get; set; }
		public bool Retired { get; set; }
		public bool UndraftedFreeAgent { get; set; }

        public List<PlayerRosterPosition>? RosterPositions { get; set; }
		
		public int CareerWins => RosterPositions?.Sum(rp => rp.TeamWins) ?? 0;
		public int CareerLosses => RosterPositions?.Sum(rp => rp.TeamLosses) ?? 0;
		public int CareerTies => RosterPositions?.Sum(rp => rp.TeamTies) ?? 0;

		public int CareerPassingYards => RosterPositions?.Sum(rp => rp.TeamPassingYards) ?? 0;
		public int CareerInterceptions => RosterPositions?.Sum(rp => rp.TeamInterceptions) ?? 0;
		public int CareerPassAttempts => RosterPositions?.Sum(rp => rp.TeamPassAttempts) ?? 0;
		public int CareerPassCompletions => RosterPositions?.Sum(rp => rp.TeamPassCompletions) ?? 0;

		public int CareerRushingAttempts => RosterPositions?.Sum(rp => rp.TeamRushingAttempts) ?? 0;
		public int CareerRushingYards => RosterPositions?.Sum(rp => rp.TeamRushingYards) ?? 0;
		public int CareerReceivingYards => RosterPositions?.Sum(rp => rp.TeamReceivingYards) ?? 0;

		public int CareerTackles => RosterPositions?.Sum(rp => rp.TeamTackles) ?? 0;
		public int CareerSacks => RosterPositions?.Sum(rp => rp.TeamSacks) ?? 0;

		public int CareerKickReturns => RosterPositions?.Sum(rp => rp.TeamKickReturns) ?? 0;
		public int CareerReturnYards => RosterPositions?.Sum(rp => rp.TeamReturnYards) ?? 0;

		public int CareerKickoffs => RosterPositions?.Sum(rp => rp.TeamKickoffs) ?? 0;
		public int CareerTouchbacks => RosterPositions?.Sum(rp => rp.TeamTouchbacks) ?? 0;

		public int CareerTouchdownsScored => RosterPositions?.Sum(rp => rp.TeamTouchdownsScored) ?? 0;
		public int CareerFieldGoalAttempts => RosterPositions?.Sum(rp => rp.TeamFieldGoalAttempts) ?? 0;
		public int CareerFieldGoalsMade => RosterPositions?.Sum(rp => rp.TeamFieldGoalsMade) ?? 0;
		public int CareerExtraPointAttempts => RosterPositions?.Sum(rp => rp.TeamExtraPointAttempts) ?? 0;
		public int CareerExtraPointsMade => RosterPositions?.Sum(rp => rp.TeamExtraPointsMade) ?? 0;
		public int CareerLongFieldGoal => RosterPositions?.Max(rp => rp.TeamLongFieldGoal) ?? 0;

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
