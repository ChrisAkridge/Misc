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

namespace EvoANTFrontend
{
	public partial class NeuronViewerForm : Form
	{
		public NeuronViewerForm(Ant ant)
		{
			InitializeComponent();
			neuronViewer1.Ant = ant;
		}
	}
}
