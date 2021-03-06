﻿//-----------------------------------------------------------------------
// <copyright file="Texture2DExtensions.cs" company="The Limitless Development Team">
//     Copyrighted under the MIT license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace SomeDungeonGame.Extensions
{
    /// <summary>
    /// Extends the Texture2D class.
    /// </summary>
    public static class Texture2DExtensions
    {
        /// <summary>
        /// Converts a Texture2D into a System.Drawing.Image.
        /// </summary>
        /// <param name="texture">The texture to convert.</param>
        /// <returns>A System.Drawing.Image containing the texture's pixels.</returns>
        public static Image ToImage(this Texture2D texture)
        {
            // Credit: http://communistgames.blogspot.com/2010/10/converting-between-texture2d-and-image.html
            if (texture == null)
            {
                return null;
            }

            if (texture.IsDisposed)
            {
                return null;
            }

            MemoryStream stream = new MemoryStream();
            texture.SaveAsPng(stream, texture.Width, texture.Height);
            stream.Seek(0, SeekOrigin.Begin);
            Image image = Bitmap.FromStream(stream);

            stream.Close();
            stream = null;
            return image;
        }
    }
}
