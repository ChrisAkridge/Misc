using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WordscapesGenerator
{
    public sealed class SparseGrid
    {
        private const int ChunkCoordinateMask = unchecked((int)0xFFFFFFF0);
        
        private readonly Dictionary<Point, SparseGridChunk> chunks;

        public byte this[int x, int y]
        {
            get
            {
                var chunkCoordinates = GetChunkCoordinates(x, y);
                return !chunks.ContainsKey(chunkCoordinates)
                    ? (byte)0
                    : chunks[chunkCoordinates][x & 0x0F, y & 0x0F];
            }
            set
            {
                var chunkCoordinates = GetChunkCoordinates(x, y);
                if (!chunks.ContainsKey(chunkCoordinates))
                {
                    chunks.Add(chunkCoordinates, new SparseGridChunk());
                }

                chunks[chunkCoordinates][x & 0x0F, y & 0x0F] = value;

                if (value == 0x00)
                {
                    // We'll sometimes make a new chunk when trying to place a word,
                    // but the placement may fail. We erase what we've written, but
                    // we still have an empty chunk that looks ugly. Here, we check
                    // for empty chunks and remove them.
                    RemoveChunkIfEmpty(chunkCoordinates);
                    // ...or let some other method do it.
                }
            }
        }

        public SparseGrid() => chunks = new Dictionary<Point, SparseGridChunk>();

        public string[] GetPrintableLines()
        {
            var gridTopEdge = chunks.MinBy(kvp => kvp.Key.Y).Key.Y;
            var gridBottomEdge = chunks.MaxBy(kvp => kvp.Key.Y).Key.Y + 15;
            var gridLeftEdge = chunks.MinBy(kvp => kvp.Key.X).Key.X;
            var gridRightEdge = chunks.MaxBy(kvp => kvp.Key.X).Key.X + 15;

            var gridWidth = gridRightEdge - gridLeftEdge;
            var gridHeight = gridBottomEdge - gridTopEdge;

            var lines = new string[gridHeight];
            var lineBuilder = new StringBuilder(gridWidth);

            var linesIndex = 0;
            for (var y = gridTopEdge; y < gridBottomEdge; y++)
            {
                for (var x = gridLeftEdge; x < gridRightEdge; x++)
                {
                    var cellCharacter = (char)(this[x, y] & 0x7F);
                    lineBuilder.Append(cellCharacter != '\0'
                        ? cellCharacter
                        : ' ');
                }

                lines[linesIndex] = lineBuilder.ToString();
                lineBuilder.Clear();
                linesIndex += 1;
            }

            return lines;
        }

        private void RemoveChunkIfEmpty(Point chunkCoordinates)
        {
            var chunk = chunks[chunkCoordinates];

            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    if (chunk[x, y] != 0) { return; }
                }
            }

            chunks.Remove(chunkCoordinates);
        }
        
        private static Point GetChunkCoordinates(int x, int y) => new Point(x & ChunkCoordinateMask, y & ChunkCoordinateMask);
    }

    internal sealed class SparseGridChunk
    {
        private readonly byte[,] cells;
        
        public int Width => 16;
        public int Height => 16;

        public byte this[int chunkX, int chunkY]
        {
            get => cells[chunkX, chunkY];
            set => cells[chunkX, chunkY] = value;
        }

        public SparseGridChunk() => cells = new byte[Width, Height];
    }

    internal readonly struct Point
    {
        public int X { get; }
        public int Y { get; }

        [DebuggerStepThrough]
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point Swap() => new Point(Y, X);

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() => HashCode.Combine(X, Y);

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>
        /// <see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object? obj) => obj is Point p && (X == p.X && Y == p.Y);

        public override string ToString() => $"{X}, {Y}";

        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);

        public static Point operator -(Point a) => new Point(-a.X, -a.Y);
    }
}
