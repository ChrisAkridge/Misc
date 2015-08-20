using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormScratchPad
{
    public partial class Form1 : Form
    {
		private double basePrice = 2.55e18;
		private int owned = 25;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
			SetWindowPos(this.Handle, this.Handle, 0, 0, 200, 200, SetWindowPosFlags.FRAMECHANGED);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Color a = Color.FromArgb(235, 235, 255);
            Color b = Color.FromArgb(255, 255, 255);
            LinearGradientBrush brush = new LinearGradientBrush(new Point(0, 0), new Point(0, 20), a, b);
            e.Graphics.FillRectangle(brush, new Rectangle(0, 0, 300, 20));
        }

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		private void button1_Click(object sender, EventArgs e)
		{
			this.basePrice = CalculatePrice(this.basePrice, 0.15d, 1);
			this.owned++;
			this.label1.Text = NumberToString(this.basePrice) + string.Format(" ({0} owned)", owned);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.basePrice = CalculatePrice(this.basePrice, 0.15d, double.Parse(this.textBox1.Text));
			this.owned += (int)double.Parse(this.textBox1.Text);
			this.label1.Text = NumberToString(this.basePrice) + string.Format(" ({0} owned)", owned);
		}

		private double CalculatePrice(double basePrice, double increaseAmount, double purchaseAmount)
		{
			return basePrice * Math.Pow(1 + increaseAmount, purchaseAmount);
		}

		private string NumberToString(double number)
		{
			string[] numberNames = {"", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion",
									"sextillion", "septillion", "octillion", "nonillion", "decillion", "undecillion",
									"duodecillion", "tredecillion", "quattourdecillion", "quindecillion", "sexdecillion",
									"septendecillion", "octodecillion", "novemdecillion", "vigintillion"};
			int level = 0;
			while (number > 1000d && level < numberNames.Length - 1)
			{
				number /= 1000d;
				level++;
			}

			if (number > 1000d)
			{
				return NumberToString(number) + " " + numberNames[level];
			}
			else
			{
				return number.ToString("#.000") + " " + numberNames[level];
			}
		}
    }

	public enum SetWindowPosFlags
	{
		NOSIZE = 0x0001,
		NOMOVE = 0x0002,
		NOZORDER = 0x0004,
		NOREDRAW = 0x0008,
		NOACTIVATE = 0x0010,
		DRAWFRAME = 0x0020,
		FRAMECHANGED = 0x0020,
		SHOWWINDOW = 0x0040,
		HIDEWINDOW = 0x0080,
		NOCOPYBITS = 0x0100,
		NOOWNERZORDER = 0x0200,
		NOREPOSITION = 0x0200,
		NOSENDCHANGING = 0x0400,
		DEFERERASE = 0x2000,
		ASYNCWINDOWPOS = 0x4000
	}
}
