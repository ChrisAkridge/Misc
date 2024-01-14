using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Collections;

public sealed class SymmetricTable<T> where T : class?
{
    private readonly List<SymmetricTableRow<T?>> rows = new List<SymmetricTableRow<T?>>();
    private readonly IEqualityComparer<T?> comparer;
    private int cellsPerRow;

    public T? this[T? key, int cellNumber]
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

    private SymmetricTable(IEqualityComparer<T?> comparer)
    {
        this.comparer = comparer;
    }

    public T? ElementAt(int index)
    {
        var rowIndex = index / cellsPerRow;
        var cellIndex = index % cellsPerRow;
        return rows[rowIndex][cellIndex];
    }

    public (T? rowKey, T? cellValue) RowAndElementAt(int index)
    {
        var rowIndex = index / cellsPerRow;
        var cellIndex = index % cellsPerRow;
        var row = rows[rowIndex];
        return (row.Key, row[cellIndex]);
    }

    public void SymmetricallyClear(T? key, int cellNumber)
    {
        var row = GetMatchingRow(key);
        var symmetricRow = GetMatchingRow(row[cellNumber]);

        row[cellNumber] = null;
        symmetricRow[cellNumber] = null;
    }

    private SymmetricTableRow<T?> GetMatchingRow(T? key)
    {
        foreach (var row in rows)
        {
            if (comparer.Equals(row.Key, key))
            {
                return row;
            }
        }

        throw new KeyNotFoundException($"Table does not contain row with key {key}");
    }

    public static SymmetricTable<T?> FromRowKeys(IEnumerable<T> rowKeys, int cellsPerRow, IEqualityComparer<T?> comparer)
    {
        var table = new SymmetricTable<T?>(comparer);

        foreach (var rowKey in rowKeys)
        {
            table.rows.Add(new SymmetricTableRow<T?>(rowKey, cellsPerRow));
        }

        table.cellsPerRow = cellsPerRow;
        return table;
    }
    
    public IEnumerable<T?> Keys => rows.Select(r => r.Key);

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        foreach (var row in rows)
        {
            sb.Append(row.Key);
            sb.Append(": ");

            for (int i = 0; i < cellsPerRow; i++)
            {
                var cellValue = row[i]?.ToString() ?? "(null)";
                sb.Append(cellValue);
                sb.Append(", ");
            }

            sb.Remove(sb.Length - 2, 2);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}