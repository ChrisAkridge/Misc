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
