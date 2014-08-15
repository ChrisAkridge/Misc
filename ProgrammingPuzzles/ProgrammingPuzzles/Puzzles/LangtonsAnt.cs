using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ProgrammingPuzzles.Puzzles
{
	public class LangtonsAnt : Puzzle
	{
		// From http://www.reddit.com/r/dailyprogrammer/comments/2c4ka3/7302014_challenge_173_intermediate_advanced/

		public override string Description
		{
			get { return "A cellular automaton. See http://en.wikipedia.org/wiki/Langton%27s_ant."; }
		}

		public override void Run()
		{
			int width = 0;
			int height = 0;
			Random random = new Random();
			Tuple<int, Color>[] colors;
			string rules = "";
			int steps = 0;

			Console.Write("Enter the board's width: ");
			width = int.Parse(Console.ReadLine());
			Console.Write("Enter the board's height: ");
			height = int.Parse(Console.ReadLine());
			Console.Write("Enter the number of colors: ");
			colors = new Tuple<int, Color>[int.Parse(Console.ReadLine())];
			Console.Write("Enter the rules for the ant ({0} rules, please): ", colors.Length);
			rules = Console.ReadLine();
			Console.Write("Enter the number of steps to run: ");
			steps = int.Parse(Console.ReadLine());

			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = new Tuple<int, Color>(i, Color.FromArgb(random.Next(int.MinValue, int.MaxValue) | unchecked((int)0xFF000000)));
			}

			Board board = new Board(width, height, colors);
			board.SetAntRules(rules);
			board.MultipleStep(steps);

			Bitmap result = board.Render();
			DateTime now = DateTime.Now;
			string fileName = string.Format("ant_{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}.png", now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
			result.Save(string.Concat(Directory.GetCurrentDirectory() + @"\", fileName), ImageFormat.Png);
			Console.WriteLine("Saved as {0}", string.Concat(Directory.GetCurrentDirectory() + @"\", fileName));
		}
	}

	public class Board
	{
		private Dictionary<int, Color> colorMap = new Dictionary<int, Color>();
		private Dictionary<int, int> antRules = new Dictionary<int, int>();
		private int maxColor;
		private Grid<int> board;
		private int antX = 0;
		private int antY = 0;
		private AntDirection antDirection = AntDirection.North;

		public Board(int width, int height, Tuple<int, Color>[] colors)
		{
			this.colorMap = new Dictionary<int, Color>();
			this.board = new Grid<int>(width, height);

			foreach (var color in colors)
			{
				colorMap.Add(color.Item1, color.Item2);
			}

			maxColor = this.colorMap.Count - 1;
		}

		public void SetAntRules(string rules)
		{
			if (rules.Length != colorMap.Count)
			{
				throw new ArgumentException();
			}

			for (int i = 0; i < rules.Length; i++)
			{
				int rule = 0;
				if (rules[i] == 'L')
				{
					rule = -1;
				}
				else if (rules[i] == 'R')
				{
					rule = 1;
				}
				else
				{
					throw new ArgumentException();
				}

				this.antRules.Add(i, rule);
			}
		}

		private void IncrementCell()
		{
			int current = this.board[this.antX, this.antY];
			if (current < this.maxColor)
			{
				current++;
			}
			else
			{
				current = 0;
			}
			this.board[antX, antY] = current;
		}

		private AntDirection GetNewDirection(int rule, AntDirection oldDirection)
		{
			if (rule == -1)
			{
				switch (oldDirection)
				{
					case AntDirection.North:
						return AntDirection.West;
					case AntDirection.South:
						return AntDirection.East;
					case AntDirection.East:
						return AntDirection.South;
					case AntDirection.West:
						return AntDirection.North;
					default:
						return AntDirection.North;
				}
			}
			else if (rule == 1)
			{
				switch (oldDirection)
				{
					case AntDirection.North:
						return AntDirection.East;
					case AntDirection.South:
						return AntDirection.West;
					case AntDirection.East:
						return AntDirection.South;
					case AntDirection.West:
						return AntDirection.North;
					default:
						return AntDirection.North;
				}
			}
			else
			{
				throw new ArgumentException();
			}
		}

		private void MoveAnt()
		{
			switch (this.antDirection)
			{
				case AntDirection.North:
					this.antY--;
					break;
				case AntDirection.South:
					this.antY++;
					break;
				case AntDirection.East:
					this.antX++;
					break;
				case AntDirection.West:
					this.antX--;
					break;
				default:
					break;
			}
		}

		public void Step()
		{
			// Is the ant in a corner?
			if (this.antX == 0 && this.antY == 0 && (this.antDirection == AntDirection.North || this.antDirection == AntDirection.West))
			{
				this.antDirection = AntDirection.East;
				this.IncrementCell();
				this.antX++;
				return;
			}
			else if (this.antX == this.board.Width - 1 && this.antY == 0 && (this.antDirection == AntDirection.North || this.antDirection == AntDirection.East))
			{
				this.antDirection = AntDirection.West;
				this.IncrementCell();
				this.antX--;
				return;
			}
			else if (this.antX == 0 && this.antY == this.board.Height - 1 && (this.antDirection == AntDirection.South || this.antDirection == AntDirection.West))
			{
				this.antDirection = AntDirection.East;
				this.IncrementCell();
				this.antX++;
				return;
			}
			else if (this.antX == this.board.Width - 1 && this.antY == this.board.Height - 1 && (this.antDirection == AntDirection.South || this.antDirection == AntDirection.East))
			{
				this.antDirection = AntDirection.West;
				this.IncrementCell();
				this.antX--;
				;
			}

			// Has the ant hit an edge?
			if (this.antX == 0 && this.antDirection == AntDirection.West)
			{
				this.antDirection = AntDirection.North;
				this.IncrementCell();
				this.antY--;
				return;
			}
			else if (this.antX == this.board.Width - 1 && this.antDirection == AntDirection.East)
			{
				this.antDirection = AntDirection.South;
				this.IncrementCell();
				this.antY++;
				return;
			}
			else if (this.antY == 0 && this.antDirection == AntDirection.North)
			{
				this.antDirection = AntDirection.West;
				this.IncrementCell();
				this.antX--;
				return;
			}
			else if (this.antY == this.board.Width - 1 && this.antDirection == AntDirection.South)
			{
				this.antDirection = AntDirection.East;
				this.IncrementCell();
				this.antX++;
				return;
			}

			int currentRule = this.antRules[this.board[this.antX, this.antY]];
			this.antDirection = this.GetNewDirection(currentRule, this.antDirection);
			this.IncrementCell();
			this.MoveAnt();
		}

		public void MultipleStep(int stepCount)
		{
			for (int i = 0; i < stepCount; i++)
			{
				this.Step();
			}
		}

		public Bitmap Render()
		{
			Bitmap result = new Bitmap(this.board.Width, this.board.Height);
			BitmapData resultData = result.LockBits(new Rectangle(0, 0, this.board.Width, this.board.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
			int stride = resultData.Stride;
			byte[] bytes = new byte[stride * result.Height];

			for (int y = 0; y < result.Height; y++)
			{
				for (int x = 0; x < result.Width; x++)
				{
					int pixelIndex = (y * result.Width) + x;
					Color color = this.colorMap[this.board[x, y]];
					bytes[y * stride + x * 4] = color.B;
					bytes[y * stride + x * 4 + 1] = color.G;
					bytes[y * stride + x * 4 + 2] = color.R;
					bytes[y * stride + x * 4 + 3] = color.A;
				}
			}

			IntPtr scan0 = resultData.Scan0;
			Marshal.Copy(bytes, 0, scan0, stride * result.Height);
			result.UnlockBits(resultData);
			return result;
		}
	}

	public class Grid<T>
	{
		private T[,] grid;
		public int Width { get; set; }
		public int Height { get; set; }

		public T this[int x, int y]
		{
			get
			{
				return this.grid[x, y];
			}
			set
			{
				this.grid[x, y] = value;
			}
		}

		public Grid(int width, int height)
		{
			this.grid = new T[width, height];
			this.Width = width;
			this.Height = height;
		}
	}

	public enum AntDirection
	{
		North,
		South,
		East,
		West
	}
}
