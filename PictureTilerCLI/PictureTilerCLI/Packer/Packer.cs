using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace PictureTilerCLI.Packer
{
    public sealed class Packer
    {
        public Node Root { get; set; }

        public void Fit(IList<Block> blocks)
        {
            Root = new Node(Point.Empty, blocks[0].Size);

            foreach (var block in blocks)
            {
                var someNode = FindNode(Root, block.Size);

                block.Fit = someNode != null ? SplitNode(someNode, block.Size) : GrowNode(block.Size);
            }
        }

        private static Node FindNode(Node someNode, Size size)
        {
            while (true)
            {
                if (someNode.Used)
                {
                    var findNode = FindNode(someNode.Right, size);

                    if (findNode != null)
                    {
                        return findNode;
                    }

                    someNode = someNode.Down;

                    continue;
                }
                else if (size.Width <= someNode.Size.Width && size.Height <= someNode.Size.Height) { return someNode; }

                return null;
            }
        }

        private static Node SplitNode(Node someNode, Size size)
        {
            someNode.Used = true;
            var (width, height) = size;

            someNode.Down = new Node(
                new Point(someNode.Location.X, someNode.Location.Y + height),
                new Size(someNode.Size.Width, someNode.Size.Height - height));
            someNode.Right = new Node(
                new Point(someNode.Location.X + width, someNode.Location.Y),
                new Size(someNode.Size.Width - width, someNode.Size.Height));

            return someNode;
        }

        private Node GrowNode(Size size)
        {
            bool canGoDown = size.Width <= Root.Size.Width;
            bool canGoRight = size.Height <= Root.Size.Height;

            bool shouldGoDown = canGoDown && (Root.Size.Width >= (Root.Size.Height + size.Height));
            bool shouldGoRight = canGoRight && (Root.Size.Height >= (Root.Size.Width + size.Width));

            return shouldGoRight
                ? GrowRight(size)
                : shouldGoDown
                    ? GrowDown(size)
                    : canGoRight
                        ? GrowRight(size)
                        : canGoDown
                            ? GrowDown(size)
                            : null;
        }

        private Node GrowRight(Size size)
        {
            var newRoot =
                new Node(Point.Empty, new Size(Root.Size.Width + size.Width, Root.Size.Height))
                {
                    Used = true,
                    Down = Root,
                    Right = new Node(new Point(Root.Size.Width, 0), new Size(size.Width, Root.Size.Height))
                };

            Root = newRoot;

            var someNode = FindNode(Root, size);

            return someNode != null ? SplitNode(someNode, size) : null;
        }

        private Node GrowDown(Size size)
        {
            var newRoot =
                new Node(Point.Empty, new Size(Root.Size.Width, Root.Size.Height + size.Height))
                {
                    Used = true,
                    Down = new Node(new Point(0, Root.Size.Height), new Size(Root.Size.Width, size.Height)),
                    Right = Root
                };

            Root = newRoot;

            var someNode = FindNode(Root, size);

            return someNode != null ? SplitNode(someNode, size) : null;
        }
    }
}
