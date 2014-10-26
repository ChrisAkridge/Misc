using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketSimulator
{
	public sealed class Game
	{
		private const double ProductPrice = 1d;
		private const double ProductMarkup = 0.2d;

		private double framesPerSecond;

		public double Bank { get; private set; }

		public int StockersEmployed { get; private set; }
		public int CashiersEmployed { get; private set; }
		public int CartPushersEmployed { get; private set; }

		public double ProductsStocked { get; private set; }
		public double CartsAvailable { get; private set; }

		private double productsPerSecondPerStocker = 1d;
		private double cartsPerSecondPerPusher = (5d / 60d);
		private double checkoutTimePerProduct = 0.1d;
		private double checkoutCompletionTime = 1d;

		public double CustomersPerSecond { get; private set; }
		public double CustomersInStore { get; private set; }
		public double ProductsPerCustomer { get; private set; }
		public double CartsPerCustomer { get; private set; }

		public double ProductsStockedPerSecond
		{
			get
			{
				return this.StockersEmployed * this.productsPerSecondPerStocker;
			}
		}

		public double ProductsBoughtPerSecond
		{
			get
			{
				return this.CustomersPerSecond * this.ProductsPerCustomer;
			}
		}

		public double CartsPerSecond
		{
			get
			{
				return (this.CartPushersEmployed * this.cartsPerSecondPerPusher);
			}
		}

		public double CartsTakenPerSecond
		{
			get
			{
				return this.CustomersPerSecond * this.CartsPerCustomer;
			}
		}

		public double TransactionsPerSecond
		{
			get
			{
				return this.CashiersEmployed * (1d / ((this.ProductsPerCustomer * this.checkoutTimePerProduct) + this.checkoutCompletionTime));
			}
		}

		public double IncomePerSecond { get; private set; }

		public Game(double framesPerSecond, double initialBank)
		{
			this.framesPerSecond = framesPerSecond;
			this.Bank = initialBank;
			this.CartsPerCustomer = 0.25d;
			this.ProductsPerCustomer = 1d;
		}

		public void Update()
		{
			double delta = 1d / this.framesPerSecond;
			this.IncomePerSecond = 0d;

			this.UpdateProductsStocked(delta);
			this.UpdateCarts(delta);
			this.UpdateCustomers(delta);
			this.UpdateCheckouts(delta);
		}

		private void UpdateProductsStocked(double delta)
		{
			double productsStockedThisFrame = this.ProductsStockedPerSecond * delta;
			double priceOfProducts = ProductPrice * productsStockedThisFrame;
			this.ProductsStocked += productsStockedThisFrame;
			this.Bank -= priceOfProducts;
			this.IncomePerSecond -= (priceOfProducts * this.framesPerSecond);
		}

		private void UpdateCarts(double delta)
		{
			double cartsPushedThisFrame = this.CartsPerSecond * delta;
			this.CartsAvailable += cartsPushedThisFrame;
		}

		private void UpdateCustomers(double delta)
		{
			double customersEnteredThisFrame = this.CustomersPerSecond * delta;
			this.CustomersInStore += customersEnteredThisFrame;

			double cartsTakenThisFrame = this.CartsPerCustomer * customersEnteredThisFrame;
			if (this.CartsAvailable >= cartsTakenThisFrame)
			{
				this.CartsAvailable -= cartsTakenThisFrame;
			}
			else
			{
				this.CustomersInStore -= customersEnteredThisFrame;
				return;
			}

			double productsBoughtThisFrame = customersEnteredThisFrame * this.ProductsPerCustomer;
			this.ProductsStocked -= productsBoughtThisFrame;
		}

		private void UpdateCheckouts(double delta)
		{
			double customerCheckoutsThisFrame = this.TransactionsPerSecond * delta;

			if (customerCheckoutsThisFrame > this.CustomersInStore)
			{
				customerCheckoutsThisFrame = this.CustomersInStore;
			}
			double spentThisFrame = customerCheckoutsThisFrame * (this.ProductsPerCustomer * (ProductPrice + (ProductPrice * ProductMarkup)));

			this.CustomersInStore -= customerCheckoutsThisFrame;
			this.Bank += spentThisFrame;
			this.IncomePerSecond += (spentThisFrame * this.framesPerSecond);
		}

		private void RecalculateCustomersPerSecond()
		{
			this.CustomersPerSecond = (this.ProductsStockedPerSecond * 0.2d) + (this.CartsPerSecond * 0.1d) * (this.TransactionsPerSecond * 0.85d);
		}

		public void HireStocker()
		{
			this.StockersEmployed++;
			this.Bank -= 15d;
			this.RecalculateCustomersPerSecond();
		}

		public void HireCashier()
		{
			this.CashiersEmployed++;
			this.Bank -= 15.5d;
			this.RecalculateCustomersPerSecond();
		}

		public void HireCartPusher()
		{
			this.CartPushersEmployed++;
			this.Bank -= 14.5d;
			this.RecalculateCustomersPerSecond();
		}
	}
}
