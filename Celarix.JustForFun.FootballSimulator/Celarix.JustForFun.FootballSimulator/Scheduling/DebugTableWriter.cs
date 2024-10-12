using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
	internal sealed class DebugTableWriter
	{
		private StringBuilder htmlBuilder;
		
		public DebugTableWriter()
		{
			htmlBuilder = new StringBuilder();
			
			// Write a basic HTML header to the builder.
			htmlBuilder.AppendLine("<!DOCTYPE html>");
			htmlBuilder.AppendLine("<html>");
			htmlBuilder.AppendLine("<head>");
			htmlBuilder.AppendLine("<title>Debug Table</title>");
			htmlBuilder.AppendLine("</head>");
			htmlBuilder.AppendLine("<body>");
		}

		public void AddUpdatedTable(GameWeekSlotType[,] table, string sectionHeader)
		{
			htmlBuilder.AppendLine($"<h1>{sectionHeader}</h1>");
			
			// Start a table in the builder.
			htmlBuilder.AppendLine("<table border=\"1\">");
			
			// We're going to rotate the table, so we'll make one row per array column (game week).
			for (var i = 0; i < table.GetLength(1); i++)
			{
				htmlBuilder.AppendLine("<tr>");
				
				// We're going to rotate the table, so we'll make one column per array row (game).
				for (var j = 0; j < table.GetLength(0); j++)
				{
					// Color the cell based on the value.
					// Empty: white
					// Assigned: green
					// PreviouslyAssigned: red
					// Ineligible: dark gray
					string cellColor = table[j, i] switch
					{
						GameWeekSlotType.Empty => "white",
						GameWeekSlotType.Assigned => "green",
						GameWeekSlotType.PreviouslyAssigned => "red",
						GameWeekSlotType.Ineligible => "darkgray",
						_ => throw new InvalidOperationException("Unknown GameWeekSlotType value.")
					};
					
					htmlBuilder.AppendLine($"<td style=\"background-color: {cellColor};\">{j}</td>");
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
