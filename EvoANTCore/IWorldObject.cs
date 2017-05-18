using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public interface IWorldObject
	{
		int PositionX { get; }
		int PositionY { get; }

		void Update();
	}
}
