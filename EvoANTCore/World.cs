using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class World
	{
		private Random random = new Random();

		public int Elapsed { get; private set; }
		public int GenerationNumber { get; private set; }
		internal int AntsAlive { get; set; }

		public int Width { get; }
		public int Height { get; }

		public List<IWorldObject> Objects { get; } = new List<IWorldObject>();
		public WorldSettings Settings { get; } = new WorldSettings(); // remove initializer

		public List<IWorldObject> ObjectsToAdd { get; } = new List<IWorldObject>();
		public List<IWorldObject> ObjectsToRemove { get; } = new List<IWorldObject>();

		private World(int width, int height, WorldSettings settings)
		{
			Width = width;
			Height = height;
			Settings = settings;
		}

		public static World CreateFirstGeneration(int width, int height, WorldSettings settings)
		{
			var world = new World(width, height, settings);
			var ants = AntCreator.CreateRandomGeneration(world);
			world.AntsAlive = ants.Count;
			world.Objects.AddRange(ants);

			// Randomize the initial positions of the ants.
			foreach (Ant ant in world.Objects)
			{
				ant.PositionX = world.random.Next(0, width);
				ant.PositionY = world.random.Next(0, height);
			}

			world.SpawnFoodAndWalls();

			return world;
		}

		private void SpawnFoodAndWalls()
		{
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					bool spawnFood = random.NextDouble() < Settings.FoodSpawnOdds;
					bool spawnWall = random.NextDouble() < Settings.WallSpawnOdds;

					if (spawnFood) { Objects.Add(new Food(x, y)); }
					else if (spawnWall) { Objects.Add(new Wall(x, y)); }
				}
			}
		}

		public void Update()
		{
			Objects.AddRange(ObjectsToAdd);
			foreach (var o in ObjectsToRemove) { Objects.Remove(o); }
			ObjectsToAdd.Clear();
			ObjectsToRemove.Clear();

			foreach (var obj in Objects)
			{
				obj.Update();
			}

			Elapsed++;

			if (AntsAlive == 0) { StartNewGeneration(); }
		}

		public IEnumerable<IWorldObject> GetObjectsAtPosition(int x, int y)
		{
			if (x < 0 || x >= Width || y < 0 || y >= Width)
			{
				return new[] { new Wall(0, 0) };
			}
			return Objects.Where(o => o.PositionX == x && o.PositionY == y);
		}
		
		public IEnumerable<IWorldObject> GetObjectsInDirection(int x, int y, Direction direction)
		{
			int newX = x, newY = y;
			if (direction == Direction.Up) { newY--; }
			else if (direction == Direction.Down) { newY++; }
			else if (direction == Direction.Left) { newX--; }
			else if (direction == Direction.Right) { newX++; }
			return GetObjectsAtPosition(newX, newY);
		}

		private IEnumerable<Tuple<Ant, Ant>> SelectTopNPairs(int pairCount)
		{
			if (pairCount * 2 > Settings.GenerationSize)
			{
				throw new ArgumentOutOfRangeException();
			}

			var ants = Objects.Where(o => o is Ant).Cast<Ant>().OrderByDescending(a => a.TimeAlive)
				.Take(pairCount * 2).ToList();
			var pairs = new List<Tuple<Ant, Ant>>(pairCount);
			
			for (int i = 0; i < pairCount; i++)
			{
				var pair = new Tuple<Ant, Ant>(ants[i], ants[i + 1]);
				pairs.Add(pair);
			}

			return pairs;
		}

		private void StartNewGeneration()
		{
			GenerationNumber++;
			Elapsed = 0;

			var topAnts = SelectTopNPairs(1);
			var newRandomAntCount = Settings.GenerationSize - (topAnts.Count());

			Objects.Clear();

			var bredAnts = AntCreator.BreedPairs(topAnts);
			var newAnts = AntCreator.CreateRandomAnts(this, newRandomAntCount);

			Objects.AddRange(bredAnts);
			Objects.AddRange(newAnts);
			AntsAlive = Settings.GenerationSize;

			foreach (var ant in Objects.Cast<Ant>())
			{
				ant.PositionX = random.Next(Width);
				ant.PositionY = random.Next(Height);
			}

			SpawnFoodAndWalls();
		}
	}
}
