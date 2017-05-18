using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public class Food : IWorldObject
	{
		public int PositionX { get; private set; }

		public int PositionY { get; private set; }

		public Food(int x, int y)
		{
			PositionX = x;
			PositionY = y;
		}

		public void Update() { }
	}
}
