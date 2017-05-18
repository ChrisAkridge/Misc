using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileElections
{
	internal static class ElectionSimulator
	{
		public static void SimulateElection(ElectoralMap map)
		{
			// Key: party
			// Value: electoral vote count
			int totalElectoralVotes = map.TotalHouseVotes + map.TotalSenateVotes;
			long votesCast = 0L;
			Random random = new Random();
			var partyPopulations = map.GetPopulationByParty().OrderByDescending(kvp => kvp.Value);
			var parties = partyPopulations.Select(p => p.Key).ToList();
			double largestPartyCandidateOdds = random.NextDouble();

			var votesByState = new Dictionary<State, Dictionary<string, long>>();
			var electoralVotesByParty = new Dictionary<string, int>();

			foreach (var party in parties)
			{
				electoralVotesByParty.Add(party, 0);
			}

			foreach (var state in map.States())
			{
				votesByState.Add(state, new Dictionary<string, long>());
				foreach (var party in parties)
				{
					votesByState[state].Add(party, 0L);
				}

				foreach (var county in state.Counties())
				{
					for (int i = 0; i < county.TotalPopulation; i++)
					{
						string votedFor = county.VoteSingle(random, parties, largestPartyCandidateOdds);
						votesByState[state][votedFor]++;
						votesCast++;
						if (votesCast % 5000L == 0) { Console.WriteLine($"{votesCast:N0} votes cast"); }
					}
				}

				string winningParty;
				int electoralVotesCast;
				GetElectoralVotes(state, votesByState[state], out electoralVotesCast, out winningParty);
				electoralVotesByParty[winningParty] += electoralVotesCast;
			}


			var votesByParty = new Dictionary<string, long>();
			foreach (var kvp in votesByState)
			{
				foreach (var partyKVP in kvp.Value)
				{
					if (!votesByParty.ContainsKey(partyKVP.Key))
					{
						votesByParty.Add(partyKVP.Key, 0L);
					}
					votesByParty[partyKVP.Key] += partyKVP.Value;
				}
			}
			
			PrintResults(totalElectoralVotes, electoralVotesByParty, votesByParty);
		}

		private static void GetElectoralVotes(State state, Dictionary<string, long> votesByParty, out int electoralVotes, out string winningParty)
		{
			var inOrder = votesByParty.OrderByDescending(kvp => kvp.Value);
			electoralVotes = state.HouseVotes + state.SenateVotes;
			winningParty = inOrder.First().Key;
		}

		private static void PrintResults(int totalElectoralVotes, Dictionary<string, int> electoralVotesByParty, IEnumerable<KeyValuePair<string, long>> popularVotesByParty)
		{
			var winner = electoralVotesByParty.OrderByDescending(kvp => kvp.Value).First();
			Console.WriteLine($"The {winner.Key} Party Wins the Election!");
			Console.WriteLine();
			Console.WriteLine("Electoral votes by party:");

			foreach (var kvp in electoralVotesByParty.OrderByDescending(kvpair => kvpair.Value))
			{
				Console.WriteLine($"{kvp.Key}: {kvp.Value} electoral vote{((kvp.Value == 1) ? "" : "s")}");
			}

			Console.WriteLine();
			Console.WriteLine("Popular votes by party:");

			foreach (var kvp in popularVotesByParty.OrderByDescending(kvpair => kvpair.Value))
			{
				Console.WriteLine($"{kvp.Key}: {kvp.Value:N0} votes");
			}
		}
	}
}
