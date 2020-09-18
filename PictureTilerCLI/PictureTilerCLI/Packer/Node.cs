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
    }
}
