using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScratchpad
{
	public sealed class Worker
	{
		private volatile bool shouldStop;

		public ConcurrentQueue<string> Queue;

		public void DoWork(string work)
		{
			this.Queue = new ConcurrentQueue<string>();
			while (!this.shouldStop)
			{
				string value = null;
				this.Queue.TryDequeue(out value);
				if (value != null)
				{
					Console.WriteLine("Dequeued {0}", value);
				}
			}
			Console.WriteLine("Terminating gracefully...");
		}

		public void RequestStop()
		{
			this.shouldStop = true;
		}
	}
}
