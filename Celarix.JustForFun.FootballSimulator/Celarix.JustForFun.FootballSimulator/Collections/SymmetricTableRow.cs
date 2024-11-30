using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections
{
    public sealed class SymmetricTableRow<T> where T : class?
    {
        private readonly T[] cells;

        public T Key { get; }

        public T this[int cellNumber]
        {
            get => cells[cellNumber];
            set => cells[cellNumber] = value;
        }

        public SymmetricTableRow(T key, int numberOfCells)
        {
            Key = key;
            cells = new T[numberOfCells];
        }
    }
}
