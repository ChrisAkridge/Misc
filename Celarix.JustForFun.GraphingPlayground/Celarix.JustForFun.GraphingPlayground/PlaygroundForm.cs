using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Celarix.JustForFun.GraphingPlayground
{
	internal partial class PlaygroundForm : Form
	{
		private readonly IPlayground playground;
		
		public PlaygroundForm(IPlayground playground)
		{
			this.playground = playground;
			InitializeComponent();
		}

		private void TSBOpenFile_Click(object sender, EventArgs e)
		{
			if (OFDMain.ShowDialog() != DialogResult.OK) { return; }

			var loadArguments = new PlaygroundLoadArguments
			{
				FilePath = OFDMain.FileName
			};
				
			playground.Load(loadArguments);
				
			var viewNames = playground.GetViewNames();

			foreach (var viewName in viewNames)
			{
				TSDDBViews.DropDownItems.Add(viewName, null, (_, _) =>
				{
					playground.GetView(viewName)(PlotMain);
				});
			}
		}
	}
}
