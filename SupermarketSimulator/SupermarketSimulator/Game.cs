using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketSimulator
{
	public sealed class Game
	{
		public double FramesPerSecond { get; private set; }
		public double SecondsPerFrame
		{
			get
			{
				return 1d / this.FramesPerSecond;
			}
		}

		public double ProductPrice { get; private set; }
		public double ProductMarkup { get; private set; }
		public double ProductsPerCustomer { get; private set; }

		public double Bank { get; private set; }

		public double CustomersPerSecond { get; private set; }
		public double CartsPushedPerSecond { get; private set; }
		public double ProductsReceivedPerSecond { get; private set; }
		public double ProductsStockedPerSecond { get; private set; }

		public double CashierScanProductTime { get; private set; }
		public double CashierFinishTransactionTime { get; private set; }
		public double TransactionTime
		{
			get
			{
				double timeToScanProducts = this.ProductsPerCustomer * this.CashierScanProductTime;
				return 1d / (timeToScanProducts + this.CashierFinishTransactionTime);
			}
		}

		public double SelfCheckoutScanProductTime
		{
			get { return this.CashierScanProductTime * 2d; }
		}
		public double SelfCheckoutFinishTransactionTime
		{
			get { return this.CashierFinishTransactionTime * 1.25d; }
		}
		public double SelfCheckoutTransactionTime
		{
			get
			{
				double timeToScanProducts = this.ProductsPerCustomer * this.SelfCheckoutScanProductTime;
				return 1d / (timeToScanProducts + this.SelfCheckoutFinishTransactionTime);
			}
		}

		public double CustomersInStore { get; private set; }
		public double ProductsReceived { get; private set; }
		public double ProductsAvailable { get; private set; }
		public double CartsAvailable { get; private set; }

		public Game(double framesPerSecond, double initialBank)
		{
			this.FramesPerSecond = framesPerSecond;
			this.Bank = initialBank;

			this.ProductPrice = 1d;
			this.ProductMarkup = 0.2d;
			this.ProductsPerCustomer = 1d;

			this.CashierScanProductTime = 0.1d;
			this.CashierFinishTransactionTime = 1d;
		}
	}
}
