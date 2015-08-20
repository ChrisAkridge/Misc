//-----------------------------------------------------------------------
// <copyright file="ColorExtensions.cs" company="The Limitless Development Team">
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
    /// Extends the Color structure.
    /// </summary>
    public static class ColorExtensions
    {
        /// <summary>
        /// Creates an RGBA color from a packed, unsigned 32-bit integer.
        /// </summary>
        /// <param name="packed">The packed, unsigned 32-bit integer.</param>
        /// <returns>The color extracted from the packed integer.</returns>
        public static Color FromPackedValue(uint packed)
        {
            // We assume that Color.PackedValue is RGBA.
            int red = (int)(packed >> 24);
            int green = (int)((packed << 8) >> 24);
            int blue = (int)((packed << 16) >> 24);
            int alpha = (int)((packed << 24) >> 24);

            return new Color(red, green, blue, alpha);
        }

        /// <summary>
        /// Serializes a color, producing a string usable by serializers/deserializers.
        /// </summary>
        /// <param name="value">The color to serialize.</param>
        /// <returns>A string in the format of "R, G, B, A".</returns>
        public static string Serialize(this Color value)
        {
            return string.Format("{0}, {1}, {2}, {3}", value.R, value.G, value.B, value.A);
        }
    }
}
