using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.GraphingPlayground.Models.Html;
using HtmlElement = Celarix.JustForFun.GraphingPlayground.Models.Html.HtmlElement;

namespace Celarix.JustForFun.GraphingPlayground.Logic
{
    internal sealed class HtmlQuilt
    {
	    public sealed class HtmlQuiltCell
		{
			public string? Name { get; init; }
			public string? HtmlColor { get; init; }
			
			public HtmlQuiltCell(string name, string htmlColor)
			{
				Name = name;
				HtmlColor = htmlColor;
			}
		}

	    private HtmlQuiltCell[] cellTypes;
		private string[] columnHeaders;
	    private string[] rowHeaders;
	    
	    private HtmlQuiltCell[,] cells;

		public string Title { get; }

		public HtmlQuilt(string title, HtmlQuiltCell[] cellTypes, string[] columnHeaders, string[] rowHeaders)
		{
			Title = title;
			this.cellTypes = cellTypes;
			this.columnHeaders = columnHeaders;
			this.rowHeaders = rowHeaders;
			
			cells = new HtmlQuiltCell[rowHeaders.Length, columnHeaders.Length];
		}
		
		public void SetCell(int row, int column, string cellTypeName)
		{
			var cellType = cellTypes.Single(ct => ct.Name == cellTypeName);
			cells[row, column] = cellType;
		}

		public string BuildHtml()
		{
			var html = new HtmlElement("html");
			var head = new HtmlElement("head");
			var metaCharset = new HtmlElement("meta")
			{
				Attributes = [new HtmlAttribute("charset", "utf-8")]
			};
			var titleElement = new HtmlElement("title")
			{
				InnerText = Title
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
							table {
								border-collapse: collapse;
							}
							
							td {
								border: 1px solid black;
								padding: 5px;
								height: 30px;
								width: 30px;
							}
							
							#legend {
								width: 20vw;
								position: fixed;
								top: 5vh;
								left: 5vw;
							}
							
							#quilt {
								margin-left: 5vw;
								margin-top: 30vh;
								width: 90vw;
							}
							"""
			};
			
			head.Children.Add(metaCharset);
			head.Children.Add(titleElement);
			head.Children.Add(metaViewport);
			head.Children.Add(style);
			html.Children.Add(head);

			var body = new HtmlElement("body");

			// Legend
			var legendDiv = new HtmlElement("div")
			{
				Attributes = [new HtmlAttribute("id", "legend")]
			};
			
			var legendTable = new HtmlElement("table");
			var legendTbody = new HtmlElement("tbody");
			var legendTrs = cellTypes.Select(ct =>
			{
				var tr = new HtmlElement("tr");
				var tdColor = new HtmlElement("td")
				{
					InnerText = ct.Name,
					Attributes = [new HtmlAttribute("style", $"background-color: {ct.HtmlColor}")]
				};
				tr.Children.Add(tdColor);
				return tr;
			});
			legendTbody.Children.AddRange(legendTrs);
			legendTable.Children.Add(legendTbody);
			
			body.Children.Add(legendDiv);
			
			// Quilt
			var quiltTable = new HtmlElement("table")
			{
				Attributes = [new HtmlAttribute("id", "quilt")]
			};
			var quiltTbody = new HtmlElement("tbody");
			var quiltTrs = new List<HtmlElement>();
			
			var columnHeaderRow = new HtmlElement("tr");
			var columnHeaderTds = columnHeaders
				.Select(ch =>
				{
					var td = new HtmlElement("td")
					{
						InnerText = ch
					};
					return td;
				})
				.Prepend(new HtmlElement("td"));
			columnHeaderRow.Children.AddRange(columnHeaderTds);
			quiltTrs.Add(columnHeaderRow);

			for (int y = 0; y < rowHeaders.Length; y++)
			{
				var rowHeader = rowHeaders[y];
				var rowTr = new HtmlElement("tr");
				var rowHeaderTd = new HtmlElement("td")
				{
					InnerText = rowHeader
				};
				rowTr.Children.Add(rowHeaderTd);

				for (int x = 0; x < columnHeaders.Length; x++)
				{
					var cell = cells[y, x];
					var cellTd = new HtmlElement("td")
					{
						Attributes = [new HtmlAttribute("style", $"background-color: {cell.HtmlColor}")]
					};
					rowTr.Children.Add(cellTd);
				}
				quiltTrs.Add(rowTr);
			}
			
			quiltTbody.Children.AddRange(quiltTrs);
			quiltTable.Children.Add(quiltTbody);
			body.Children.Add(quiltTable);
			html.Children.Add(body);

			return html.PrintSelf(0);
		}
	}
}
