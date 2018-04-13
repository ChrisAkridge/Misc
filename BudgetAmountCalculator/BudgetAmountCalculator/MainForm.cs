using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BudgetAmountCalculator
{
	public partial class MainForm : Form
	{
		private string totalAmountLastText = "";
		private string currentAmountLastTest = "";
		private bool inEventHandler = false;

		public MainForm()
		{
			InitializeComponent();
		}

		private static bool ValidAmountInput(string input)
		{
			bool seenDot = false;
			foreach (char c in input)
			{
				if (c == '.')
				{
					if (seenDot) { return false; }
					else { seenDot = true; }
				}
				else if (!char.IsDigit(c))
				{
					return false;
				}
			}
			return true;
		}

		private void Amount_TextChanged(object sender, EventArgs e)
		{
			if (inEventHandler) { return; }
			inEventHandler = true;

			var textBox = (TextBox)sender;

			if (!ValidAmountInput(textBox.Text))
			{
				textBox.Text = totalAmountLastText;
				textBox.SelectionStart = textBox.Text.Length;
				textBox.SelectionLength = 0;
			}
			else
			{
				totalAmountLastText = textBox.Text;
			}

			inEventHandler = false;
		}

		private void ButtonCalculate_Click(object sender, EventArgs e)
		{
			var totalAmount = decimal.Parse(TextTotalAmount.Text);
			var currentAmount = decimal.Parse(TextCurrentAmount.Text);
			decimal remainder = totalAmount - currentAmount;

			if (DTPByDate.Value < DateTime.Now)
			{
				LabelBudgetAmount.Text = "Date selected is before today.";
				return;
			}

			if (remainder <= 0m)
			{
				LabelBudgetAmount.Text = "Budget amount reached!";
			}
			else
			{
				var weeksUntilDue = (DTPByDate.Value - DateTime.Now).TotalDays / 7d;
				decimal budgetAmount = remainder / (decimal)weeksUntilDue;
				LabelBudgetAmount.Text = $"Budget ${budgetAmount:F2}";
			}
		}
	}
}
