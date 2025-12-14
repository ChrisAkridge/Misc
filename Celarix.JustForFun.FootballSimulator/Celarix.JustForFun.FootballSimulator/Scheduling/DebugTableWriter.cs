using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Collections;
using Celarix.JustForFun.FootballSimulator.Random;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
	internal sealed class DebugTableWriter
	{
		private StringBuilder htmlBuilder;
		private readonly IRandom random;
		
		public DebugTableWriter(IRandom random)
		{
			htmlBuilder = new StringBuilder();
			this.random = random;

            // Write a basic HTML header to the builder.
            htmlBuilder.AppendLine("<!DOCTYPE html>");
			htmlBuilder.AppendLine("<html>");
			htmlBuilder.AppendLine("<head>");
			htmlBuilder.AppendLine("<title>Debug Table</title>");
			htmlBuilder.AppendLine("</head>");
			htmlBuilder.AppendLine("<body>");
		}

		public void AddUpdatedTable(SymmetricTable<BasicTeamInfo?> table, string sectionHeader)
		{
			htmlBuilder.AppendLine($"<h1>{sectionHeader}</h1>");
			
			// Start a table in the builder.
			htmlBuilder.AppendLine("<table border=\"1\">");
			
			// Write a table header with cells " ", "1", "2", "3", and "4".
			htmlBuilder.AppendLine("<tr>");
			htmlBuilder.AppendLine("<th> </th>");
			for (int i = 0; i < 4; i++)
			{
				htmlBuilder.AppendLine($"<th>{i + 1}</th>");
			}
			htmlBuilder.AppendLine("</tr>");
			
			// For each row of the table, write the team name and then four cells, either empty if null, or the opponent if present.
			// Choose a random background color for the symmetrically connected cells.
			var randomColorsForCells = new Dictionary<(BasicTeamInfo? team, int index), string>();
			foreach (var team in table.Keys)
			{
				htmlBuilder.AppendLine("<tr>");
				htmlBuilder.AppendLine($"<td>{team?.Name ?? "null"}</td>");
				for (int i = 0; i < 4; i++)
				{
					var opponent = table[team, i];

					if (opponent == null)
					{
						htmlBuilder.AppendLine("<td> </td>");
					}
					else
					{
						string randomColor;

						if (randomColorsForCells.ContainsKey((team, i)))
						{
							randomColor = randomColorsForCells[(team, i)];
						}
						else
						{
							randomColor = $"#{random.Next(0x1000000):X6}";
							randomColorsForCells.Add((opponent, i), randomColor);
						}
						htmlBuilder.AppendLine($"<td style=\"background-color:{randomColor}\">{opponent.Name}</td>");
					}
				}
				htmlBuilder.AppendLine("</tr>");
			}
			
			// End the table.
			htmlBuilder.AppendLine("</table>");
		}

		public void WriteHTMLToFile()
		{
			// Close the document.
			htmlBuilder.AppendLine("</body>");
			htmlBuilder.AppendLine("</html>");
			
			const string filePath =
				@"F:\Documents\GitHub\Misc\Celarix.JustForFun.FootballSimulator\Celarix.JustForFun.FootballSimulator.Console\bin\Debug\net6.0\debug\gameWeekTable.html";
			File.WriteAllText(filePath, htmlBuilder.ToString());
		}
	}
}
