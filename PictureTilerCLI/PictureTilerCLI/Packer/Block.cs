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
    }
}
