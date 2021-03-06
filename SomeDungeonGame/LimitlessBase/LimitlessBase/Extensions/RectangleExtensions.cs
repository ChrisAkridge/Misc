﻿//-----------------------------------------------------------------------
// <copyright file="RectangleExtensions.cs" company="The Limitless Development Team">
//     Copyrighted unter the MIT Public License.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SomeDungeonGame.Extensions
{
    /// <summary>
    /// Extends the Rectangle struct.
    /// </summary>
    public static class RectangleExtensions
    {
        /// <summary>
        /// Draws the outline of a rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the outline.</param>
        public static void DrawOutline(this Rectangle rect, Color color)
        {
            SpriteBatchExtensions.DrawRectangleEdges(GameServices.SpriteBatch, rect, color);
        }
    }
}
