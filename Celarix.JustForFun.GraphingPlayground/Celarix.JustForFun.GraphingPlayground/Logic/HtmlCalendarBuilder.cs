using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models.Html;
using HtmlElement = Celarix.JustForFun.GraphingPlayground.Models.Html.HtmlElement;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
	internal static class HtmlCalendarBuilder
	{
		public static string BuildHtml(string title, Dictionary<DateOnly, string> dayStyles, Dictionary<string, string> legend)
		{
			var html = new HtmlElement("html");
			var head = new HtmlElement("head");
			var metaCharset = new HtmlElement("meta")
			{
				Attributes = [new HtmlAttribute("charset", "utf-8")]
			};
			var titleElement = new HtmlElement("title")
			{
				InnerText = title
			};
			var metaViewport = new HtmlElement("meta")
			{
				Attributes =
				[
					new HtmlAttribute("name", "viewport"),
					new HtmlAttribute("content", "width=device-width, initial-scale=1")
				]
			};
			var style = new HtmlElement("style")
			{
				InnerText = """
				            td {
				                border: 1px solid black;
				                padding: 5px;
				                height: 30px;
				            }
				            
				            .weekday {
				                font-weight: bold;
				            }
				            
				            #calendar {
				                width: 1000px;
				                display: flex;
				                flex-direction: row;
				                flex-wrap: wrap;
				            }
				            """
			};
			
			head.Children.Add(metaCharset);
			head.Children.Add(titleElement);
			head.Children.Add(metaViewport);
			head.Children.Add(style);
			html.Children.Add(head);

			var body = new HtmlElement("body");

			var bodyDiv = new HtmlElement("div")
			{
				Attributes = [new HtmlAttribute("id", "calendar")]
			};

			var (minYear, minMonth, _) = dayStyles.Keys.Min();
			var (maxYear, maxMonth, _) = dayStyles.Keys.Max();

			var monthDivs = new List<HtmlElement>();

			for (var year = minYear; year <= maxYear; year++)
			{
				for (var month = year == minYear ? minMonth : 1;
				     year == maxYear ? month <= maxMonth : month <= 12;
				     month++)
				{
					monthDivs.Add(GetTableForSingleMonth(year, month, dayStyles));
				}
			}

			bodyDiv.Children.Add(BuildLegend(legend));
			bodyDiv.Children.AddRange(monthDivs);
			
			body.Children.Add(bodyDiv);
			html.Children.Add(body);

			return html.PrintSelf(0);
		}

		private static HtmlElement GetTableForSingleMonth(int year, int month, Dictionary<DateOnly, string> dayStyles)
		{
			var monthName = DateTimeFormatInfo.CurrentInfo.GetMonthName(month);

			var div = new HtmlElement("div");
			
			var h3 = new HtmlElement("h3")
			{
				InnerText = $"{monthName} {year}"
			};

			var table = new HtmlElement("table");
			var tbody = new HtmlElement("tbody");
			var weekdayTr = new HtmlElement("tr");
			var weekdayTds = new[]
			{
				"S", "M", "T", "W", "T",
				"F", "S"
			}
			.Select(w => new HtmlElement("td")
			{
				Attributes = [new HtmlAttribute("class", "weekday")],
				InnerText = w
			});
			weekdayTr.Children.AddRange(weekdayTds);
			tbody.Children.Add(weekdayTr);

			string?[,] dayCellTexts = new string[7, 6];
			string?[,] dayCellStyles = new string[7, 6];
			var firstDayOfMonth = new DateOnly(year, month, 1);
			var lastDayOfMonth = DateTime.DaysInMonth(year, month);
			var y = 0;
			var x = (int)firstDayOfMonth.DayOfWeek;

			for (var i = 1; i <= lastDayOfMonth; i++)
			{
				var date = new DateOnly(year, month, i);
				
				dayCellTexts[x, y] = i.ToString();

				if (dayStyles.TryGetValue(date, out var style))
				{
					dayCellStyles[x, y] = style;
				}

				if (x == 6)
				{
					x = 0;
					y++;
				}
				else
				{
					x++;
				}
			}

			var currentTdRow = new HtmlElement[7];
			for (y = 0; y < 6; y++)
			{
				for (x = 0; x < 7; x++)
				{
					var dayText = dayCellTexts[x, y];
					currentTdRow[x] = new HtmlElement("td")
					{
						InnerText = dayText ?? "" 
					};

					if (dayCellStyles[x, y] != null)
					{
						currentTdRow[x].Attributes.Add(new HtmlAttribute("style", dayCellStyles[x, y]!));
					}
					else if (dayText == null)
					{
						currentTdRow[x].Attributes.Add(new HtmlAttribute("style", "background-color: gray;"));
					}
				}

				var currentTr = new HtmlElement("tr");
				currentTr.Children.AddRange(currentTdRow);
				tbody.Children.Add(currentTr);
				currentTdRow = new HtmlElement[7];
			}
			
			table.Children.Add(tbody);
			
			div.Children.Add(h3);
			div.Children.Add(table);

			return div;
		}
		
		private static HtmlElement BuildLegend(Dictionary<string, string> legend)
		{
			var legendDiv = new HtmlElement("div");
			var legendTitle = new HtmlElement("h3")
			{
				InnerText = "Legend"
			};
			var legendTable = new HtmlElement("table");

			foreach (var kvp in legend)
			{
				var tr = new HtmlElement("tr");
				var td = new HtmlElement("td")
				{
					Attributes = [new HtmlAttribute("style", kvp.Value)],
					InnerText = kvp.Key
				};
				tr.Children.Add(td);
				legendTable.Children.Add(tr);
			}
			
			legendDiv.Children.Add(legendTitle);
			legendDiv.Children.Add(legendTable);

			return legendDiv;
		}
	}
}
