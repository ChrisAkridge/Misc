using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SupermarketSimulatorTest
{
	public partial class Form1 : Form
	{
		private Game game;

		public Form1()
		{
			InitializeComponent();
			this.game = new Game((1f / 30f), 100000m);
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			this.game.Update();

			this.LabelBank.Text = string.Format("Bank: {0:C2}", game.Bank);
			this.LabelCustomersPerSecond.Text = string.Format("Customers per second: {0}", game.CustomersPerSecond);
			this.LabelCustomersInStore.Text = string.Format("Customers in store: {0}", game.CustomersInStore);
			this.LabelIncomePerSecond.Text = string.Format("Income per second: {0:C2}", game.IncomePerSecond);
			this.LabelStockedProducts.Text = string.Format("Stocked products: {0}", Math.Floor(game.StockedProducts));

			if (game.Bank <= 0m)
			{
				this.Timer.Enabled = false;
				MessageBox.Show("You went bankrupt!");
				this.Close();
			}
		}

		private void ButtonBuyStocker_Click(object sender, EventArgs e)
		{
			this.game.HireStocker();
			this.ButtonBuyStocker.Text = string.Format("Stockers: {0}", this.game.StockersEmployed);
		}
	}
}
