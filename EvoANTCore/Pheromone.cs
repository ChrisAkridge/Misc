using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public class Pheromone : IWorldObject
	{
		public int PositionX { get; private set; }

		public int PositionY { get; private set; }

		public Pheromone(int positionX, int positionY)
		{
			PositionX = positionX;
			PositionY = positionY;
		}

		public void Update() { }
	}
}
