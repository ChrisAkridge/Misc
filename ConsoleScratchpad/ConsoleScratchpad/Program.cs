using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleScratchpad
{
	class Program
	{
		static unsafe void Main(string[] args)
		{
			int i = 5;
			int* j = &i;
			int** k = &j;
			int*** l = &k;
			int**** m = &l;

			****m = 137;
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

			for (int i = 0; i < length; i++)
			{
				string memoryAddress = string.Format("0x{0:X8}", (int)pb);
				string byteValue = string.Format("{0:X2}", *pb);
				string asciiValue = string.Format("{0}", (char)*pb);

				Console.WriteLine(string.Format("{0}: {1} == {2}", memoryAddress, byteValue, asciiValue));
				pb++;
			}
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
