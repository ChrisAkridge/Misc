using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections
{
    public sealed class SymmetricTableRow<T>(T key, int numberOfCells) where T : class?
    {
        private readonly T[] cells = new T[numberOfCells];

        public T Key { get; } = key;

        public T this[int cellNumber]
        {
            get => cells[cellNumber];
            set => cells[cellNumber] = value;
        }
    }
}
