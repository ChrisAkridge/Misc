﻿//-----------------------------------------------------------------------
// <copyright file="IPositionable.cs" company="The Limitless Development Team">
//     Copyrighted under the MIT license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace SomeDungeonGame.Interfaces
{
    /// <summary>
    /// Defines an object with a position and a size.
    /// </summary>
    public interface IPositionable
    {
        /// <summary>
        /// Gets the position of this object.
        /// </summary>
        Vector2 Position { get; }

        /// <summary>
        /// Gets the size of this object.
        /// </summary>
        Vector2 Size { get; }
    }
}
