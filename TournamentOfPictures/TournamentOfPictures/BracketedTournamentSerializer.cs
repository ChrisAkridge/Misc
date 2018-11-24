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
		public static string SerializeString(BracketedTournament<string> tournament) => JObject.FromObject(tournament.GetSerializableObjects()).ToString();

		public static BracketedTournament<string> Deserialize(string json)
		{
			var obj = JObject.Parse(json);

			var standingsArray = (JArray)obj["standings"];
			var previousRoundsArray = (JArray)obj["previousRounds"];
			var currentRoundObject = (JObject)obj["currentRound"];

			var result = new BracketedTournament<string>();
			var standings = new Dictionary<string, int>();
			result.CurrentRoundNumber = (int)obj["currentRoundNumber"];
			
			foreach (var jToken in standingsArray)
			{
				var standingsObject = (JObject)jToken;
				string key = (string)standingsObject["team"];
				int value = (int)standingsObject["wins"];
				standings.Add(key, value);
			}
			result.SerializationSetStandings(standings);

			foreach (var jToken in previousRoundsArray)
			{
				var previousRoundObject = (JObject)jToken;
				result.SerializationAddPreviousRound(DeserializeRound(result, previousRoundObject));
			}

			result.SerializationSetCurrentRound(DeserializeRound(result, currentRoundObject));
			result.OverflowItem = (string)obj["overflowItem"];
			return result;
		}

		public static BracketedTournament<string> FromFile(string filePath) => Deserialize(File.ReadAllText(filePath));

		private static BracketedTournamentRound<string> DeserializeRound(BracketedTournament<string> tournament, JObject obj)
		{
			var matches = new List<BracketedTournamentMatch<string>>();
			var matchArray = (JArray)obj["matches"];

			foreach (JToken matchObj in matchArray)
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
