using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CharacterManager;

namespace CustomCharEdit
{
	public partial class MainForm : Form
	{
		private Panel[,] panels;
		private bool[,] characterPixels;
		private CharacterBank bank;

		private byte[] PatternBytes
		{
			get
			{
				if (characterPixels == null)
				{
					throw new ArgumentNullException("characterPixels");
				}

				byte[] result = new byte[8];
				for (int i = 0; i < 8; i++)
				{
					if (characterPixels[0, i]) result[i] |= 0x10;
					if (characterPixels[1, i]) result[i] |= 0x08;
					if (characterPixels[2, i]) result[i] |= 0x04;
					if (characterPixels[3, i]) result[i] |= 0x02;
					if (characterPixels[4, i]) result[i] |= 0x01;
				}

				return result;
			}
		}
		
		public MainForm()
		{
			InitializeComponent();

			panels = new Panel[5,8];
			int controlX = 16;
			int controlY = 140;
			int panelSize = 24;

			for (int x = 0; x < 5; x++)
			{
				for (int y = 0; y < 8; y++)
				{
					// Create each panel and place it on the form.
					Panel panel = new Panel();
					panel.Name = string.Format("panelX{0}Y{1}", x, y);
					panel.Location = new Point(controlX, controlY);
					panel.Size = new Size(panelSize, panelSize);
					panel.BackColor = Color.White;
					panel.Click += panel_Click;
					Controls.Add(panel);

					panels[x, y] = panel;
					controlY += panelSize;
				}

				controlY = 140;
				controlX += panelSize;
			}

			characterPixels = new bool[5,8];
			bank = new CharacterBank(@"../../../LCDBoardControllerMain/bin/Debug/charbank.bnk");
		}

		private void UpdateTextboxes()
		{
			byte[] pattern = PatternBytes;
			StringBuilder bytesBuilder = new StringBuilder();
			
			foreach (var patternByte in pattern)
			{
				bytesBuilder.Append(string.Format("0x{0:X2}, ", patternByte));
			}
			bytesBuilder.Remove(46, 2);

			string bytes = bytesBuilder.ToString();

			TextboxPatternBytes.Text = bytes;
			TextConstructor.Text = string.Format(@"new CustomCharacter({0});", bytes);
		}

		private void panel_Click(object sender, EventArgs e)
		{
			Panel panel = (Panel)sender;
			bool setOrClear = ((MouseEventArgs)e).Button == System.Windows.Forms.MouseButtons.Left;
			int x, y;
			GetPanelNumber(panel.Name, out x, out y);

			if (setOrClear)
			{
				panel.BackColor = Color.Black;
				characterPixels[x, y] = true;
			}
			else
			{
				panel.BackColor = Color.White;
				characterPixels[x, y] = false;
			}

			UpdateTextboxes();
		}

		private void GetPanelNumber(string panelName, out int x, out int y)
		{
			// Panel name: panelX0Y0 where 0 is the panel's X/Y index
			// Trim the first characters

			panelName = panelName.Substring(6);
			string[] numbers = panelName.Split('Y');

			x = int.Parse(numbers[0]);
			y = int.Parse(numbers[1]);
		}

		private void ButtonSave_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(TextCharacterName.Text) || bank[TextCharacterName.Text] != null)
			{
				MessageBox.Show("A character by this name already exists in the bank.", "Character Already Exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}

			bank.Add(TextCharacterName.Text, new CustomCharacter(PatternBytes));
			TextCharacterName.Text = "";
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			bank.WriteToFile(@"../../../LCDBoardControllerMain/bin/Debug/charbank.bnk");
		}
	}
}
