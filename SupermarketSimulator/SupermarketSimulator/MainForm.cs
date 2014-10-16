using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SupermarketSimulator
{
	public partial class MainForm : Form
	{
		private Game game;
		private bool within = false;

		public MainForm()
		{
			InitializeComponent();

			this.game = new Game(30d, 100000d);
		}

		private void ListBoxBuyingOptions_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (!within)
			{
				within = true;
				switch (this.ListBoxBuyingOptions.SelectedIndex)
				{
					case 0:
						// Stocker
						this.game.HireStocker();
						this.ListBoxBuyingOptions.Items[0] = string.Format("Stockers ({0})", this.game.StockersEmployed);
						break;
					case 1:
						// Cashier
						this.game.HireCashier();
						this.ListBoxBuyingOptions.Items[1] = string.Format("Cashiers ({0})", this.game.CashiersEmployed);
						break;
					case 2:
						// Cart pusher
						this.game.HireCartPusher();
						this.ListBoxBuyingOptions.Items[2] = string.Format("Cart Pushers ({0})", this.game.CartPushersEmployed);
						break;
					default:
						break;
				}
				within = false;
			}
		}

		private void ConstructData()
		{
			string message = string.Format(
				@"Bank: {0:C2}

Customers Per Second: {1}
Customers In Store: {2}

Carts Pushed Per Second: {3}
Carts Taken Per Second: {4}
Carts Available: {5}

Products Stocked Per Second: {6}
Products Bought Per Second: {7}
Products Available: {8}

Transactions Per Second: {9}
Income Per Second: {10:C2}",
						this.game.Bank,
						this.game.CustomersPerSecond,
						Math.Floor(this.game.CustomersInStore),
						this.game.CartsPerSecond,
						this.game.CartsTakenPerSecond,
						Math.Floor(this.game.CartsAvailable),
						this.game.ProductsStockedPerSecond,
						this.game.ProductsBoughtPerSecond,
						Math.Floor(this.game.ProductsStocked),
						this.game.TransactionsPerSecond,
						this.game.IncomePerSecond
				);

			this.TextBoxGameInformation.Text = message;
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			this.game.Update();
			this.ConstructData();
		}
	}
}
