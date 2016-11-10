using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileElections
{
	internal sealed class ElectoralMap
	{
		private List<State> states = new List<State>();

		public int StateCount => states.Count;
		public int CountyCount => states.Sum(s => s.CountyCount);
		public int TotalHouseVotes => states.Sum(s => s.HouseVotes);
		public int TotalSenateVotes => states.Count * 2;

		public ElectoralMap(TreeNode root)
		{
			if (!root.Children.Any(r => r.NodeType == NodeType.Folder))
			{
				// No folders, so make one state and give it one county
				states.Add(new State(root));
			}

			var topFolders = root.Children.Where(c => c.NodeType == NodeType.Folder);

			foreach (var topFolder in topFolders)
			{
				states.Add(new State(topFolder));
			}
		}

		public IEnumerable<KeyValuePair<string, long>> GetPopulationByParty()
		{
			var result = new Dictionary<string, long>();
			var statePopulations = states.Select(s => s.GetPopulationByParty());
			foreach (var statePop in statePopulations)
			{
				foreach (var party in statePop)
				{
					if (!result.ContainsKey(party.Key)) { result.Add(party.Key, 0); }
					result[party.Key] += party.Value;
				}
			}
			return result.Select(kvp => kvp);
		}
	}

	internal class State
	{
		private List<County> counties = new List<County>();

		public int HouseVotes
		{
			get
			{
				return (int)counties.Sum(c => c.TotalPopulation / 733103);
			}
		}

		public int CountyCount => counties.Count;
		public int SenateVotes => 2;

		public State(TreeNode stateNode)
		{
			PopulateCounties(stateNode);
		}

		private void PopulateCounties(TreeNode node)
		{
			var folders = node.Children.Where(c => c.NodeType == NodeType.Folder);

			foreach (var folder in folders)
			{
				PopulateCounties(folder);
			}

			counties.Add(new County(node));
		}

		public IEnumerable<KeyValuePair<string, long>> GetPopulationByParty()
		{
			var result = new Dictionary<string, long>();
			foreach (var county in counties)
			{
				foreach (var party in county.PopulationEnumerator())
				{
					if (!result.ContainsKey(party.Key)) { result.Add(party.Key, 0); }
					result[party.Key] += party.Value;
				}
			}
			return result.Select(kvp => kvp);
		}
	}

	internal class County
	{
		private Dictionary<string, long> populationByParty = new Dictionary<string, long>();	

		public string Name { get; private set; }
		public long TotalPopulation => populationByParty.Sum(p => p.Value);
		
		public County(TreeNode node)
		{
			foreach (string file in node.Children.Where(c => c.NodeType == NodeType.File).Select(c => c.NodeItem))
			{
				string extension = Path.GetExtension(file).ToUpperInvariant();
				if (!populationByParty.ContainsKey(extension)) { populationByParty.Add(extension, 0); }

				var fileSize = new FileInfo(file).Length;
				populationByParty[extension] += (fileSize / 1000);
			}
		}

		public IEnumerable<KeyValuePair<string, long>> PopulationEnumerator()
		{
			foreach (var kvp in populationByParty)
			{
				yield return kvp;
			}
		}
	}
}
