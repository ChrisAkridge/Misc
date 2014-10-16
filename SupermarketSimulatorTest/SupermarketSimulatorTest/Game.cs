using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketSimulatorTest
{
	public sealed class Game
	{
		private const int ProductsPerCustomer = 1;
		private const decimal PricePerProduct = 1.0m;
		private const decimal Markup = 0.2m;

		private float secondsPerFrame;
		private int updatesUntilWagePaid;

		// Stockers stock 1 product per second, which is 0.33 products per frame.
		// It doesn't make sense for a stocker to stock one product.
		private float partialProductStockingCache;
		private float partialCustomersInStoreCache;

		public decimal Bank { get; private set; }
		public double CustomersPerSecond { get; private set; }
		public double CustomersInStore { get; private set; }

		public float StockedProducts { get; private set; }
		public int StockersEmployed { get; private set; }

		public int CashiersEmployed { get; private set; }
		public float FramesPerTransaction
		{
			get
			{
				float framesPerSecond = 1f / this.secondsPerFrame;
				float transactionTimeInSeconds = (ProductsPerCustomer * 0.1f) + 1f;
				return transactionTimeInSeconds / framesPerSecond;
			}
		}

		public float TransactionsPerSecond
		{
			get
			{
				return 1f / ((ProductsPerCustomer * 0.1f) + 1f);
			}
		}

		public decimal IncomePerSecond
		{
			get
			{
				decimal income = ((decimal)(this.CustomersPerSecond * ProductsPerCustomer) * (PricePerProduct + (PricePerProduct * Markup)));
				decimal outflow = (this.StockersEmployed * PricePerProduct);
				return income - outflow;
			}
		}

		public Game(float secondsPerFrame, decimal initialBank)
		{
			this.Bank = initialBank;
			this.secondsPerFrame = secondsPerFrame;
		}

		public void Update()
		{
			if (this.updatesUntilWagePaid > 0)
			{
				this.updatesUntilWagePaid--;
			}
			else
			{
				this.Bank -= (StockersEmployed * 7.25m) * (CashiersEmployed * 7.5m);
				this.updatesUntilWagePaid = this.GetFramesBetweenWagesPaid();
			}

			// Stock one product per stocker.
			float productsStockedThisFrame = this.StockersEmployed * secondsPerFrame;
			if (productsStockedThisFrame % 1 != 0)
			{
				this.partialProductStockingCache += productsStockedThisFrame % 1;
				productsStockedThisFrame = (float)Math.Floor(productsStockedThisFrame);

				if (this.partialProductStockingCache > 1)
				{
					productsStockedThisFrame += (float)Math.Floor(this.partialProductStockingCache);
					partialProductStockingCache -= (float)Math.Floor(this.partialProductStockingCache);
				}
			}

			this.Bank -= (decimal)((float)PricePerProduct * productsStockedThisFrame);
			this.StockedProducts += productsStockedThisFrame;

			// Simulate customers buying products.
			float customersInStoreThisFrame = (float)this.CustomersPerSecond * secondsPerFrame;
			if (customersInStoreThisFrame % 1 != 0)
			{
				this.partialCustomersInStoreCache += customersInStoreThisFrame % 1;
				customersInStoreThisFrame = (float)Math.Floor(customersInStoreThisFrame);

				if (this.partialCustomersInStoreCache > 1)
				{
					customersInStoreThisFrame += (float)Math.Floor(this.partialCustomersInStoreCache);
					partialCustomersInStoreCache -= (float)Math.Floor(this.partialCustomersInStoreCache);
				}
			}
		}

		private void RecalculateCustomersPerSecond()
		{
			this.CustomersPerSecond = (StockersEmployed * 0.2d) + (CashiersEmployed * 0.1d);
		}

		private int GetFramesBetweenWagesPaid()
		{
			return (int)((1f / this.secondsPerFrame) * 10f);
		}

		public void HireStocker()
		{
			this.StockersEmployed++;
			this.Bank -= 14.5m;
			this.RecalculateCustomersPerSecond();
		}

		public void HireCashier()
		{
			this.CashiersEmployed++;
			this.Bank -= 15m;
			this.RecalculateCustomersPerSecond();
		}
	}
}
