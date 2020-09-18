using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleScratchpad.DealReentrancy;

using static System.Console;

namespace ConsoleScratchpad
{
	
	[StructLayout(LayoutKind.Sequential)]
	public struct Structure
	{
		public int x;
		public int y;
		public int z;
	}

	class Program
	{
		static void Main(string[] args)
        {
			string[] table = TableMaker.MakeTable();
			File.WriteAllLines(@"D:\Documents\CarsQuickBuy\13661\completeTable5.csv", table);
        }

		private static void DoNothing(int i) { }

		private static unsafe void CountUniqueFloats()
		{
			var floatStrings = new HashSet<string>();
			uint ui = 0u;
			float* pf = (float*)&ui;

			do
			{
				floatStrings.Add((*pf).ToString());
				ui++;
				if (ui % 100000 == 0) Console.WriteLine(ui);
			} while (ui != 0u);

			Console.WriteLine($"{floatStrings.Count} unique out of {uint.MaxValue} ({(floatStrings.Count / uint.MaxValue) * 100f}%)");
		}

        private static void StringInterpolationTest()
        {
            string world = "World!";
            int num = 137;
            int numPadding = 2456;
            uint numHex = 0xDEADBEEF;
            float decimalNum = 3.14159f;
            DateTime dateTime = DateTime.Now;

            WriteLine($"Hello, {world}!");
            WriteLine($"Tonight's lucky number is {num}!");
            WriteLine($"I like zeroes, so have {numPadding:D12}");
            WriteLine($"Hex is nice, wouldn't you say so, Mr. {numHex:X8}?");
            WriteLine($"Mmm. Non-floor pie. {decimalNum:F3}");
            WriteLine($"It's currently {dateTime}.");
            WriteLine($"Expressions? {((dateTime.Second < 30) ? "After 30s" : "Before 30s")}! You bet!");
            WriteLine($"Method calls? {GenString()} Sure! Why not?");
            WriteLine($"Most expressions supported. {((3 + 5 < 12) ? 8 : 4)}");

            ReadKey(intercept: true);
        }

        private static string GenString()
        {
            Random random = new Random();
            return $"{random.Next()}, {random.Next()}";
        }

        private static unsafe void ThreadWork()
        {
            Worker workerObject = new Worker();
            Thread workerThread = new Thread(() => workerObject.DoWork("worker"));


            workerObject.Queue = new System.Collections.Concurrent.ConcurrentQueue<string>();
            workerThread.Start();
            Console.WriteLine("Main thread: starting work...");

            while (workerObject.Queue == null) { }

            for (int i = 0;i < 10000;i++)
            {
                workerObject.Queue.Enqueue(i.ToString());
                Thread.Sleep(1);
            }

            Console.WriteLine("{0} objects remaining enqueued", workerObject.Queue.Count);

            workerObject.RequestStop();
            workerThread.Join();
            Console.WriteLine("Main thread: worker thread has terminated");
            Console.ReadLine();
        }

        private static unsafe void DoStuff()
		{
			for (int i = 0;i < 10;i++)
			{
				Console.WriteLine("Hello World #{0}!", i);
			}
		}

		private static unsafe void* GetPointer<T>(T item) where T : class
		{
			TypedReference reference = __makeref(item);
			return (void*)(**(IntPtr**)&reference);
		}

		private static unsafe void SwapTypeCodes<T1, T2>(T1 a, T2 b)
			where T1 : class
			where T2 : class
		{
			// THIS IS PURE EVIL
			// DO NOT EVER EVER USE IT IN PRODUCTION CODE

			TypedReference ra = __makeref(a);
			TypedReference rb = __makeref(b);

			void* a_ptr = (void*)(**(IntPtr**)&ra);
			void* b_ptr = (void*)(**(IntPtr**)&rb);

			int* a_typecode_ptr = (int*)a_ptr;
			int* b_typecode_ptr = (int*)b_ptr;

			int temp = *a_typecode_ptr;
			*a_typecode_ptr = *b_typecode_ptr;
			*b_typecode_ptr = temp;
		}

		private static unsafe void SwapReferences<T1, T2>(T1 a, T2 b)
			where T1 : class
			where T2 : class
		{
			// THIS IS ALSO PURE EVIL
			TypedReference ra = __makeref(a);
			TypedReference rb = __makeref(b);

			int* a_ref = (int*)*(IntPtr*)&ra;
			int* b_ref = (int*)*(IntPtr*)&rb;

			int temp = *a_ref;
			*a_ref = *b_ref;
			*b_ref = temp;
		}

		private static unsafe void PrintMemory(void* ptr, int length)
		{
			byte* pb = (byte*)ptr;

			for (int i = 0;i < length;i++)
			{
				string memoryAddress = string.Format("0x{0:X8}", (int)pb);
				string byteValue = string.Format("{0:X2}", *pb);
				string asciiValue = string.Format("{0}", (char)*pb);

				Console.WriteLine(string.Format("{0}: {1} == {2}", memoryAddress, byteValue, asciiValue));
				pb++;
			}

			ReadKey(intercept: true);
		}

		private static unsafe string ChangeStringLength(string s, int length)
		{
			// ALSO PURE EVIL
			// DON'T USE THIS
			TypedReference rs = __makeref(s);
			void* s_ptr = (void*)((byte*)(**(IntPtr**)&rs) + 4);
			int* s_length_ptr = (int*)s_ptr;
			*s_length_ptr = length;
			return s;
		}

		private static void Whatever()
		{

		}
	}
}
