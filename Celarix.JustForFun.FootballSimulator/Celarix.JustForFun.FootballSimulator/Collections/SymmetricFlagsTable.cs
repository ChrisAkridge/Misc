using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections
{
    public sealed class SymmetricFlagsTable<T, TCell>
        where T : class?
        where TCell : struct, Enum
    {
        private readonly T?[] keys;
        private readonly TCell?[,] flags;
        private readonly IEqualityComparer<T?> comparer;

        public TCell? this[T? row, T? column]
        {
            get => flags[IndexOf(row), IndexOf(column)];
            set
            {
                flags[IndexOf(row), IndexOf(column)] = value;
                flags[IndexOf(column), IndexOf(row)] = value;
            }
        }
        
        public IReadOnlyList<T?> Keys => keys;

        public SymmetricFlagsTable(T?[] keys, IEqualityComparer<T?> comparer)
        {
            this.keys = keys;
            this.comparer = comparer;
            flags = new TCell?[keys.Length, keys.Length];
        }

        public int CountRow(T? key)
        {
            var rowIndex = IndexOf(key);
            var setCount = 0;

            for (var i = 0; i < keys.Length; i++)
            {
                setCount += (flags[rowIndex, i] != null) ? 1 : 0;
            }

            return setCount;
        }

        public int CountColumn(T? key)
        {
            var columnIndex = IndexOf(key);
            var setCount = 0;

            for (var i = 0; i < keys.Length; i++) { setCount += (flags[i, columnIndex] != null) ? 1 : 0; }

            return setCount;
        }

        public IEnumerable<(T? key, TCell cell)> GetNonNullCellsForRow(T? rowKey)
        {
            var rowIndex = IndexOf(rowKey);

            for (int cell = 0; cell < keys.Length; cell++)
            {
                if (flags[rowIndex, cell] != null) { yield return (keys[cell], flags[rowIndex, cell]!.Value); }
            }
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var keyStrings = keys.Select(k => k?.ToString() ?? "null").ToArray();
            var longestKeyStringLength = keyStrings.Max(k => k.Length);
            
            builder.Append(' ', longestKeyStringLength + 1);

            foreach (var keyString in keyStrings)
            {
                builder.Append(keyString + ' ');
            }
            
            builder.AppendLine();

            for (var row = 0; row < keyStrings.Length; row++)
            {
                var rowString = keyStrings[row];
                builder.Append(rowString.PadRight(longestKeyStringLength + 1));

                for (var cell = 0; cell < keyStrings.Length; cell++)
                {
                    var cellString = keyStrings[cell];
                    var padLength = cellString.Length + 1;
                    var flag = (flags[row, cell] != null) ? "X" : ".";
                    builder.Append(flag.PadRight(padLength));
                }
                
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private int IndexOf(T? key)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                if (comparer.Equals(keys[i], key)) { return i; }
            }
            
            throw new InvalidOperationException($"Key {key} not found in the table");
        }
    }
}
