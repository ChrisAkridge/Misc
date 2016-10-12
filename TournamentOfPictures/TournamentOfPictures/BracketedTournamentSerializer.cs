using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TournamentOfPictures
{
	internal static class BracketedTournamentSerializer
	{
		public static string SerializeString(BracketedTournament<string> tournament)
		{
			return JObject.FromObject(tournament.GetSerializableObjects()).ToString();
		}

		public static BracketedTournament<string> Deserialize(string json)
		{
			JObject obj = JObject.Parse(json);

			JArray standingsArray = (JArray)obj["standings"];
			JArray previousRoundsArray = (JArray)obj["previousRounds"];
			JObject currentRoundObject = (JObject)obj["currentRound"];

			BracketedTournament<string> result = new BracketedTournament<string>();
			Dictionary<string, int> standings = new Dictionary<string, int>();
			result.CurrentRoundNumber = (int)obj["currentRoundNumber"];
			
			foreach (JObject standingsObject in standingsArray)
			{
				string key = (string)standingsObject["team"];
				int value = (int)standingsObject["wins"];
				standings.Add(key, value);
			}
			result.SerializationSetStandings(standings);

			foreach (JObject previousRoundObject in previousRoundsArray)
			{
				result.SerializationAddPreviousRound(DeserializeRound(result, previousRoundObject));
			}

			result.SerializationSetCurrentRound(DeserializeRound(result, currentRoundObject));
			result.OverflowItem = (string)obj["overflowItem"];
			return result;
		}

		public static BracketedTournament<string> FromFile(string filePath)
		{
			return Deserialize(File.ReadAllText(filePath));
		}
		
		private static BracketedTournamentRound<string> DeserializeRound(BracketedTournament<string> tournament, JObject obj)
		{
			List<BracketedTournamentMatch<string>> matches = new List<BracketedTournamentMatch<string>>();
			JArray matchArray = (JArray)obj["matches"];

			foreach (var matchObj in matchArray)
			{
				matches.Add(DeserializeMatch((JObject)matchObj));
			}

			return new BracketedTournamentRound<string>(tournament, matches);
		}

		private static BracketedTournamentMatch<string> DeserializeMatch(JObject obj)
		{
			string team1 = (string)obj["team1"];
			string team2 = (string)obj["team2"];
			int winnerIndex = (int)obj["winnerIndex"];

			return new BracketedTournamentMatch<string>(team1, team2, winnerIndex);
		}
	}
}
