using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public class Wall : IWorldObject
	{
		public int PositionX { get; private set; }

		public int PositionY { get; private set; }

		public Wall(int x, int y)
		{
			PositionX = x;
			PositionY = y;
		}

		public void Update() { }
	}
}
