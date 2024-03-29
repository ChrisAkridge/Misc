﻿using ICSharpCode.Decompiler.IL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.FinalProcessing
{
	internal readonly struct Point
	{
		public static Point Empty => new Point(0, 0);

		public int X { get; }
		public int Y { get; }

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	internal readonly struct Size
	{
		public int Width { get; }
		public int Height { get; }

		public Size(int width, int height)
		{
			Width = width;
			Height = height;
		}

		public void Deconstruct(out int width, out int height)
		{
			width = Width;
			height = Height;
		}
	}

	internal sealed class Node
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

	internal sealed class Block
	{
		public Size Size { get; set; }
		public Node Fit { get; set; }
		public string CanvasFolderPath { get; set; }
	}

	internal sealed class Packer
	{
		public Node Root { get; set; }

		public void Fit(IList<Block> blocks, IProgress<string> progress)
		{
			Root = new Node(Point.Empty, blocks[0].Size);

			for (var i = 0; i < blocks.Count; i++)
			{
				var block = blocks[i];
				var someNode = FindNode(Root, block.Size);

				block.Fit = someNode != null ? SplitNode(someNode, block.Size) : GrowNode(block.Size);

				if (i % 100 == 0)
				{
					progress.Report($"Placed image {i + 1} of {blocks.Count}");
				}
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

				if (size.Width <= someNode.Size.Width && size.Height <= someNode.Size.Height) { return someNode; }

				return null;
			}
		}
		private static Node SplitNode(Node someNode, Size size)
		{
			someNode.Used = true;
			(int width, int height) = size;

			someNode.Down = new Node(
				new Point(someNode.Location.X, someNode.Location.Y + height),
				new Size(someNode.Size.Width, someNode.Size.Height - height));
			someNode.Right = new Node(
				new Point(someNode.Location.X + width, someNode.Location.Y),
				new Size(someNode.Size.Width - width, size.Height));

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