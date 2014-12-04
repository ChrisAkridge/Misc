using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DualStackMemory
{
	public class MemorySpace : IDisposable
	{
		public int StackSize { get; private set; }
		private IntPtr sizeStackBasePointer;
		private IntPtr sizeStackTopPointer;
		private IntPtr memoryStackBasePointer;
		private IntPtr memoryStackTopPointer;

		public MemorySpace(int stackSize, int memoryStackSize)
		{
			this.StackSize = stackSize;
			this.sizeStackBasePointer = Marshal.AllocHGlobal(this.StackSize * 4);
			this.sizeStackTopPointer = this.sizeStackBasePointer;
			this.memoryStackBasePointer = Marshal.AllocHGlobal(memoryStackSize);
			this.memoryStackTopPointer = this.memoryStackBasePointer;
		}

		public void Dispose()
		{
			Marshal.FreeHGlobal(sizeStackBasePointer);
			Marshal.FreeHGlobal(memoryStackBasePointer);
		}
	}
}
