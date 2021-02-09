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
                Console.WriteLine($"==== Placing {block.Size.Width}, {block.Size.Height}");
                var someNode = FindNode(Root, block.Size);

                block.Fit = someNode != null ? SplitNode(someNode, block.Size) : GrowNode(block.Size);
                
                var blockRect = new Rectangle(block.Fit.Location, block.Size);

                var otherBlockRects = blocks.Except(new[] { block })
                    .Where(b => b.Fit != null)
                    .Select(b => new Rectangle(b.Fit.Location, b.Size));

                if (otherBlockRects.Any(r => r.IntersectsWith(blockRect)))
                {
                    throw new Exception("Block collided :(");
                }
            }
        }

        private static Node FindNode(Node someNode, Size size)
        {
            Console.WriteLine($"Finding node for {size.Width}, {size.Height}");
            Console.WriteLine($"Starting from: {someNode}");

            while (true)
            {
                if (someNode.Used)
                {
                    Console.WriteLine($"Trying right node");
                    var findNode = FindNode(someNode.Right, size);

                    if (findNode != null)
                    {
                        return findNode;
                    }

                    Console.WriteLine("Right node was not usable; trying down node");
                    someNode = someNode.Down;

                    continue;
                }
                else if (size.Width <= someNode.Size.Width && size.Height <= someNode.Size.Height)
                {
                    return someNode;
                }

                Console.WriteLine("No usable node found");

                return null;
            }
        }

        private static Node SplitNode(Node someNode, Size size)
        {
            Console.WriteLine($"Using existing node {someNode} for {size.Width}, {size.Height}");
            someNode.Used = true;
            var (width, height) = size;

            someNode.Down = new Node(
                new Point(someNode.Location.X, someNode.Location.Y + height),
                new Size(someNode.Size.Width, someNode.Size.Height - height));
            Console.WriteLine($"New down node: {someNode.Down}");
            someNode.Right = new Node(
                new Point(someNode.Location.X + width, someNode.Location.Y),
                new Size(someNode.Size.Width - width, size.Height));
            Console.WriteLine($"New right node: {someNode.Right}");

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
            Console.WriteLine($"Growing right by {size.Width}");
            var newRoot =
                new Node(Point.Empty, new Size(Root.Size.Width + size.Width, Root.Size.Height))
                {
                    Used = true,
                    Down = Root,
                    Right = new Node(new Point(Root.Size.Width, 0), new Size(size.Width, Root.Size.Height))
                };
            
            Console.WriteLine($"New root node {newRoot} with old root as down node");
            Console.WriteLine($"New right node {newRoot.Right}");

            Root = newRoot;

            var someNode = FindNode(Root, size);

            return someNode != null ? SplitNode(someNode, size) : null;
        }

        private Node GrowDown(Size size)
        {
            Console.WriteLine($"Growing down by {size.Height}");
            var newRoot =
                new Node(Point.Empty, new Size(Root.Size.Width, Root.Size.Height + size.Height))
                {
                    Used = true,
                    Down = new Node(new Point(0, Root.Size.Height), new Size(Root.Size.Width, size.Height)),
                    Right = Root
                };

            Console.WriteLine($"New root node {newRoot} with old root as right node");
            Console.WriteLine($"New down node {newRoot.Right}");

            Root = newRoot;

            var someNode = FindNode(Root, size);

            return someNode != null ? SplitNode(someNode, size) : null;
        }
    }
}
