﻿//-----------------------------------------------------------------------
// <copyright file="SizedGrid.cs" company="The Limitless Development Team">
//     Copyrighted unter the MIT Public License.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SomeDungeonGame.Interfaces;
using SomeDungeonGame.Physics;

namespace SomeDungeonGame.Collections
{
    /// <summary>
    /// Represents a grid made of cells of a specified size.
    /// </summary>
    /// <typeparam name="T">A type that derives from the <see cref="IPositionable"/> interface.</typeparam>
    public sealed class SizedGrid<T> where T : IPositionable
    {
        /*
         * The sized grid is a generic collection composed of cells.
         * Each cell is of a certain size (usually pixels), and each
         * cell holds a reference to an object. Objects are required
         * to implement the IPositionable interface. If an object is
         * larger than a grid cell, the object is placed within multiple
         * grid cells, with each cell holding a reference to the object.
         * If the object is smaller, the cell can still only hold one reference
         * to it.
         */

        /// <summary>
        /// The internal grid.
        /// </summary>
        private Grid<T> grid;

        /// <summary>
        /// Gets the width of a grid cell, usually in pixels.
        /// </summary>
        public int CellWidth { get; private set; }

        /// <summary>
        /// Gets the height of a grid cell, usually in pixels.
        /// </summary>
        public int CellHeight { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SizedGrid{T}"/> class.
        /// </summary>
        /// <param name="cellWidth">The width of the grid cells.</param>
        /// <param name="cellHeight">The height of the grid cells.</param>
        /// <param name="gridWidth">The width of the grid in cells.</param>
        /// <param name="gridHeight">The height of the grid in cells.</param>
        public SizedGrid(int cellWidth, int cellHeight, int gridWidth, int gridHeight)
        {
            if (cellWidth <= 0 || cellHeight <= 0)
            {
                throw new Exception(string.Format("SizedGrid<T>.ctor(int, int, int, int): Cell width and height must be greater than zero. Cell Width: {0}, Cell Height: {1}, Grid Width: {2}, Grid Height: {3}", cellWidth, cellHeight, gridWidth, gridHeight));
            }
            else if (gridWidth <= 0 || gridHeight <= 0)
            {
                throw new Exception(string.Format("SizedGrid<T>.ctor(int, int, int, int): Grid width and height must be greater than zero. Cell Width: {0}, Cell Height: {1}, Grid Width: {2}, Grid Height: {3}", cellWidth, cellHeight, gridWidth, gridHeight));
            }

            this.grid = new Grid<T>(gridWidth, gridHeight);
            this.CellWidth = cellWidth;
            this.CellHeight = cellHeight;
        }

        /// <summary>
        /// Gets an object on the grid from the specified cell.
        /// To add objects to the grid, use the Add method.
        /// </summary>
        /// <param name="x">The X-coordinate of the cell.</param>
        /// <param name="y">The Y-coordinate of the cell.</param>
        /// <returns>The object at the specified grid cell, or null if there is no object in the cell.</returns>
        public T this[int x, int y]
        {
            get
            {
                // TODO: add bounds check
                return this.grid[x, y];
            }

            private set
            {
                // TODO: add bounds check
                this.grid[x, y] = value;
            }
        }

        /// <summary>
        /// Adds an item to the grid.
        /// WARNING: This will overwrite any items that are
        /// already present where the item will be placed.
        /// </summary>
        /// <param name="item">The item to add to the grid.</param>
        public void Add(IPositionable item)
        {
            if (item.Position.X % this.CellWidth != 0 || item.Position.Y % this.CellHeight != 0)
            {
                // TODO: add exception here
            }
            else if (item.Size.X % this.CellWidth != 0 || item.Size.Y % this.CellHeight != 0)
            {
                // TODO: ditto
            }

            var startingCell = new IntVector2((int)item.Position.X / this.CellWidth, (int)item.Position.Y / this.CellHeight);
            int widthInCells = (int)item.Size.X / this.CellWidth;
            int heightInCells = (int)item.Size.Y / this.CellHeight;

            for (int y = startingCell.Y; y < startingCell.Y + heightInCells; y++)
            {
                for (int x = startingCell.X; x < startingCell.X + widthInCells; x++)
                {
                    if ((IPositionable)this[x, y] != null)
                    {
                        this.Remove(this[x, y]);
                    }

                    this[x, y] = (T)item;
                }
            }
        }

        /// <summary>
        /// Removes an item from the grid.
        /// </summary>
        /// <param name="item">The item to be removed from the grid.</param>
        public void Remove(IPositionable item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "SizedGrid<T>.Remove(IPositionable): Cannot remove a null reference from the grid.");
            }
            else if (!this.IndexWithinBounds((int)item.Position.X, (int)item.Position.Y))
            {
                throw new Exception(string.Format("SizedGrid<T>.Remove(IPositionable): Cannot remove an item that does not fall within the grid. X:{0}, Y:{1}", item.Position.X, item.Position.Y));
            }

            var startingCell = new IntVector2((int)item.Position.X / this.CellWidth, (int)item.Position.Y / this.CellHeight);
            int widthInCells = (int)item.Size.X / this.CellWidth;
            int heightInCells = (int)item.Size.Y / this.CellHeight;

            for (int y = startingCell.Y; y < startingCell.Y + heightInCells; y++)
            {
                for (int x = startingCell.X; x < startingCell.X + widthInCells; x++)
                {
                    this[x, y] = default(T);
                }
            }
        }

        // Exception Checking Methods

        /// <summary>
        /// Checks if a grid cell coordinate falls within the bounds of the grid.
        /// </summary>
        /// <param name="x">The X-coordinate of the coordinate to check.</param>
        /// <param name="y">The Y-coordinate of the coordinate to check.</param>
        /// <returns>True if the cell falls within the grid, false if otherwise.</returns>
        private bool IndexWithinBounds(int x, int y)
        {
            return (x >= 0 && x < (this.grid.Width * this.CellWidth)) && (y >= 0 && y < (this.grid.Height * this.CellHeight));
        }

        /// <summary>
        /// Checks if a point in space falls within the bounds of the grid.
        /// </summary>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point falls within the grid, false if otherwise.</returns>
        private bool PointWithinBounds(Vector2 point)
        {
            int rightEdge = this.grid.Width * this.CellWidth;
            int bottomEdge = this.grid.Height * this.CellHeight;

            return (point.X >= 0f && point.X <= rightEdge) && (point.Y >= 0f && point.Y <= bottomEdge);
        }
    }
}
