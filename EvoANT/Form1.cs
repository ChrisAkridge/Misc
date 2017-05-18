using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvoANTCore;

namespace EvoANTCore
{
	public partial class Form1 : Form
	{
		World world;

		public Form1()
		{
			InitializeComponent();

			// http://stackoverflow.com/a/77233/2709212
			var prop = typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance);
			prop.SetValue(displayGrid1, true);

			WorldSettings settings = new WorldSettings();
			world = World.CreateFirstGeneration(16, 16, settings);
			displayGrid1.World = world;
		}

		private void ButtonUpdate_Click(object sender, EventArgs e)
		{
			world.Update();
			displayGrid1.Invalidate();
		}
	}
}
