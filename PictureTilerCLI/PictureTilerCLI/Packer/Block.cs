using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace PictureTilerCLI.Packer
{
    public sealed class Block
    {
        public Size Size { get; set; }
        public Node Fit { get; set; }
        public string ImageFilePath { get; set; }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"({Size.Width}, {Size.Height}) at {(Fit != null ? $"{Fit.Location.X}, {Fit.Location.Y}" : "null")}";
    }
}
