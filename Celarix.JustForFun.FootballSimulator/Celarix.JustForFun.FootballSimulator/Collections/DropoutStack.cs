using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections
{
    internal class DropoutStack<T> : IEnumerable<T>
    {
        private T[] items;
        private int count;

        public DropoutStack(int capacity)
        {
            items = new T[capacity];
            count = 0;
        }

        public void Push(T item)
        {
            if (count == items.Length)
            {
                Array.Copy(items, 1, items, 0, items.Length - 1);
                items[items.Length - 1] = item;
            }
            else
            {
                items[count] = item;
                count++;
            }
        }

        public T Pop()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            count--;
            return items[count];
        }

        public T Peek()
        {
            if (count == 0)
            {
                throw new InvalidOperationException("The stack is empty.");
            }

            return items[count - 1];
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Get the generic enumerator from the array.
            return ((IEnumerable<T>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();
    }
}
