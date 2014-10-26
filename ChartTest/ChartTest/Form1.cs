using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChartTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string[] lines = this.textBox1.Text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			List<double> values = new List<double>(lines.Length);

			foreach (string line in lines)
			{
				values.Add(double.Parse(line));
			}

			List<double> averages = new List<double>(values.Count);
			for (int i = 0; i < values.Count; i++)
			{
				double average = 0d;
				for (int j = 0; j < i; j++)
				{
					average += values[j];
				}
				average /= (i == 0) ? 1 : i;
				averages.Add(average);
			}

			this.chart1.Series[0].Points.Clear();
			StringBuilder builder = new StringBuilder();
			foreach (double average in averages)
			{
				this.chart1.Series[0].Points.Add(average);
				builder.Append(average);
				builder.Append(Environment.NewLine);
			}

			this.textBox1.Text = builder.ToString();
		}
	}
}
