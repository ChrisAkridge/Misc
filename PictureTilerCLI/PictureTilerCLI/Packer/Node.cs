using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace PictureTilerCLI.Packer
{
    public sealed class Node
    {
        public bool Used { get; set; }
        public Node Down { get; set; }
        public Node Right { get; set; }
        public Size Size { get; set; }
        public Point Location { get; set; }

        public Node(Point location, Size size)
        {
            Size = size;
            Location = location;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"Node {Size.Width}, {Size.Height} at {Location.X}, {Location.Y} ({(Right == null ? "no right" : $"right at {Right.Location.X}, {Right.Location.Y}")}, {(Down == null ? "no down" : $"Down at {Down.Location.X}, {Down.Location.Y}")})";
    }
}
