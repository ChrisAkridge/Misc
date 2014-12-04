using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DualStackMemory
{
	public class StackType
	{
		public abstract ObjectSizeType SizeType { get; }
		public abstract int ObjectSize { get; }
	}

	public enum ObjectSizeType
	{
		Static,
		Dynamic
	}
}
