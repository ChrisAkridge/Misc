using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupermarketSimulator.Buyables
{
	public abstract class Buyable
	{
		public double BasePrice { get; protected set; }
		public double ExpansionRate { get; protected set; }
		public double AmountOwned { get; protected set; }

		public abstract double CustomerPerSecondIncrease { get; }
		public abstract double ProductsReceivedPerSecondIncrease { get; }
		public abstract double ProductsStockedPerSecondIncrease { get; }
		public abstract double CartsPerSecondIncrease { get; }

		public double Price
		{
			get
			{
				return this.BasePrice * Math.Pow(this.ExpansionRate, this.AmountOwned);
			}
		}
		
		public abstract void Buy(int amount)
		{
			this.AmountOwned += amount;
		}
	}
}
