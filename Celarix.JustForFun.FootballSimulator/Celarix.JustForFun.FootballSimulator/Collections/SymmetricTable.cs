using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections
{
    public sealed class SymmetricTable<T> where T : class
    {
        private readonly List<SymmetricTableRow<T>> rows = new List<SymmetricTableRow<T>>();
        private IEqualityComparer<T> comparer;
        private int cellsPerRow;

        public T this[T key, int cellNumber]
        {
            get
            {
                var row = GetMatchingRow(key);
                return row[cellNumber];
            }
            set
            {
                var row = GetMatchingRow(key);
                var symmetricRow = GetMatchingRow(value);

                row[cellNumber] = value;
                symmetricRow[cellNumber] = key;
            }
        }

        public int TotalCells => rows.Count * cellsPerRow;

        private SymmetricTable() { }

        public T ElementAt(int index)
        {
            var rowIndex = index / cellsPerRow;
            var cellIndex = index % cellsPerRow;
            return rows[rowIndex][cellIndex];
        }

        public (T rowKey, T cellValue) RowAndElementAt(int index)
        {
            var rowIndex = index / cellsPerRow;
            var cellIndex = index % cellsPerRow;
            var row = rows[rowIndex];
            return (row.Key, row[cellIndex]);
        }

        private SymmetricTableRow<T> GetMatchingRow(T key)
        {
            foreach (var row in rows)
            {
                if (comparer!.Equals(row.Key, key))
                {
                    return row;
                }
            }

            throw new KeyNotFoundException($"Table does not contain row with key {key}");
        }

        public static SymmetricTable<T> FromRowKeys(IEnumerable<T> rowKeys, int cellsPerRow, IEqualityComparer<T> comparer)
        {
            var table = new SymmetricTable<T>();

            foreach (var rowKey in rowKeys)
            {
                table.rows.Add(new SymmetricTableRow<T>(rowKey, cellsPerRow));
            }

            table.cellsPerRow = cellsPerRow;
            table.comparer = comparer;
            return table;
        }
    }
}
