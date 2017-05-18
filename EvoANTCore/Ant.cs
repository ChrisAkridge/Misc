using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class Ant : IWorldObject
	{
		private const int FoodLifespanIncrease = 10;
		private const int MaxFullness = 40;
		private const int FoodFullnessIncrease = 5;
		private const int InputNeuronCount = 7;
		private const int OutputNeuronCount = 6;

		public Direction FacingDirection { get; private set; }

		public int PositionX { get; internal set; }

		public int PositionY { get; internal set; }

		public World World { get; private set; }

		// Input Neurons
		private Neuron foodPresent;
		private Neuron wallInFront;
		private Neuron wallBehind;
		private Neuron wallToTheLeft;
		private Neuron wallToTheRight;
		private Neuron pheromonePresent;
		private Neuron hungerNeuron;
		public IReadOnlyList<Neuron> InputNeurons { get; private set; }

		// Hidden Layer Neurons
		public NeuronLayer HiddenLayer { get; set; }

		// Output Neurons
		private Neuron eatFood;
		private Neuron moveForward;
		private Neuron moveBackward;
		private Neuron moveLeft;
		private Neuron moveRight;
		private Neuron placePheromone;
		public IReadOnlyList<Neuron> OutputNeurons { get; private set; }

		public int RemainingLifespan { get; private set; }
		public int TimeAlive { get; private set; }
		public int Fullness { get; private set; }

		internal Ant(World world)
		{
			this.World = world;
			RemainingLifespan = world.Settings.InitialAntLifespan;

			// Initialize the values for the input neurons.
			// You can tweak the exact values later, if you like.
			foodPresent = new Neuron(10d, 10d, 1d, 0.8d, "inFoodPresent");
			wallInFront = new Neuron(10d, 10d, 1d, 0.8d, "inWallInFront");
			wallBehind = new Neuron(10d, 10d, 1d, 0.8d, "inWallBehind");
			wallToTheLeft = new Neuron(10d, 10d, 1d, 0.8d, "inWallToTheLeft");
			wallToTheRight = new Neuron(10d, 10d, 1d, 0.8d, "inWallToTheRight");
			pheromonePresent = new Neuron(10d, 10d, 1d, 0.8d, "inPheromonePresent");
			hungerNeuron = new Neuron(30d, 10d, 0.6d, 1.1d, "inHunger");

			// Do the same for the output neurons.
			eatFood = new Neuron(10d, 10d, 1d, 0.8d, "outEatFood");
			moveForward = new Neuron(10d, 10d, 1d, 0.8d, "outMoveForward");
			moveBackward = new Neuron(10d, 10d, 1d, 0.8d, "outMoveBackward");
			moveLeft = new Neuron(10d, 10d, 1d, 0.8d, "outMoveLeft");
			moveRight = new Neuron(10d, 10d, 1d, 0.8d, "outMoveRight");
			placePheromone = new Neuron(10d, 10d, 1d, 0.8d, "outPlacePheromone");

			InputNeurons = new List<Neuron>
			{
				foodPresent, wallInFront, wallBehind, wallToTheLeft, wallToTheRight, pheromonePresent,
				hungerNeuron
			}.AsReadOnly();

			OutputNeurons = new List<Neuron>
			{
				eatFood, moveForward, moveBackward, moveLeft, moveRight, placePheromone
			}.AsReadOnly();

			// Wire up the output neurons.
			eatFood.OnFire = EatFood;
			moveForward.OnFire = () => Move(Direction.Up);
			moveBackward.OnFire = () => Move(Direction.Down);
			moveLeft.OnFire = () => Move(Direction.Left);
			moveRight.OnFire = () => Move(Direction.Right);
			placePheromone.OnFire = PlacePheromone;
		}

		public void Update()
		{
			// Dead ants don't do anything.
			if (RemainingLifespan <= 0) { return; }

			foreach (var neuron in InputNeurons) { neuron.ClearAfterTimestep(); }
			foreach (var neuron in HiddenLayer.Neurons) { neuron.ClearAfterTimestep(); }
			foreach (var neuron in OutputNeurons) { neuron.ClearAfterTimestep(); }

			// Check to see if any input neurons can fire by checking for stimuli.
			if (CheckForFood()) { foodPresent.Fire(); }
			if (CheckForWallInDirection(Direction.Up)) { wallInFront.Fire(); }
			if (CheckForWallInDirection(Direction.Down)) { wallBehind.Fire(); }
			if (CheckForWallInDirection(Direction.Left)) { wallToTheLeft.Fire(); }
			if (CheckForWallInDirection(Direction.Right)) { wallToTheRight.Fire(); }
			if (CheckForPheromoneHere()) { pheromonePresent.Fire(); }
			if (Fullness < (MaxFullness / 2)) { hungerNeuron.Fire(); }

			// Reduce the ant's remaining lifespan and fullness.
			RemainingLifespan--;
			TimeAlive++;
			if (Fullness > 0) { Fullness--; }
			if (RemainingLifespan <= 0) { Die(); }
		}

		private IEnumerable<Food> GetFoodInFrontOfAnt() =>
			World.GetObjectsInDirection(PositionX, PositionY, FacingDirection).Where(o => o is Food)
				.Cast<Food>();

		// Input Neuron Checks
		private bool CheckForFood() => GetFoodInFrontOfAnt().Any();

		private bool CheckForWallInDirection(Direction direction) =>
			World.GetObjectsInDirection(PositionX, PositionY, direction).Any(o => o is Wall);

		private bool CheckForPheromoneHere() =>
			World.GetObjectsAtPosition(PositionX, PositionY).Any(o => o is Pheromone);

		// Output Neuron Behaviors
		private void EatFood()
		{
			// Check if there is food in the direction the ant is facing
			var foodNearAnt = GetFoodInFrontOfAnt();
			if (foodNearAnt.Any())
			{
				var food = foodNearAnt.First();
				// Eat only one piece of food on each neuron firing.
				RemainingLifespan += FoodLifespanIncrease;
				Fullness += FoodFullnessIncrease;
				if (Fullness > MaxFullness)
				{
					// The ant overate. Kill it!
					RemainingLifespan = 0;
					Die();
				}

				// Remove the food now that we've eaten it.
				World.ObjectsToRemove.Add(food);
			}
		}

		private void Move(Direction direction)
		{
			// We can move iff the input neurons for the walls are not firing.

			if (direction == Direction.Up)
			{
				if (!wallInFront.IsFiring) { PositionY--; FacingDirection = Direction.Up; }
			}
			else if (direction == Direction.Down)
			{
				if (!wallBehind.IsFiring) { PositionY++; FacingDirection = Direction.Down; }
			}
			else if (direction == Direction.Left)
			{
				if (!wallToTheLeft.IsFiring) { PositionX--; FacingDirection = Direction.Left; }
			}
			else if (direction == Direction.Right)
			{
				if (!wallToTheRight.IsFiring) { PositionX++; FacingDirection = Direction.Right; }
			}
		}

		private void PlacePheromone()
		{
			var pheromone = new Pheromone(PositionX, PositionY);
			World.ObjectsToAdd.Add(pheromone);
		}

		private void Die()
		{
			World.AntsAlive--;
		}

		// Breeding Functions
		internal void RewireConnectionsToOutputNeurons()
		{
			// When two ants breed, a copy of half of each ant's neurons is taken from them and
			// placed in a new ant. Connections to neurons that weren't taken are removed, except
			// for those that connect to output neurons. Here we reconnect them to the same output
			// neurons in the new ant.

			foreach (var neuron in HiddenLayer.Neurons)
			{
				var connectionsToOutputNeurons = neuron.OutboundConnections.Where(o => o.IsOutputNeuron)
					.ToList();

				var newConnections = connectionsToOutputNeurons.Select(o =>
				{
					string specialName = o.SpecialName;
					return OutputNeurons.First(n => n.SpecialName == o.SpecialName);
				}).ToList();

				neuron.OutboundConnections.RemoveAll(n => connectionsToOutputNeurons.Contains(n));

				neuron.OutboundConnections.AddRange(newConnections);
			}
		}

		internal void RewireConnectionsToInputNeurons(Ant father, Ant mother)
		{
			// When neurons are copied for breeding, they are copied by reference. Thus, both the
			// parent and the child hold a reference to the same neuron on the heap. Since each
			// neuron only tracks outbound, not inbound, connections, a neuron doesn't know what
			// neurons connect to it, only what neurons it connects to. However, since the neurons
			// are copied by reference, we just need to take the outbound connections list from
			// the father and the mother, remove any connections to neurons that aren't in the child,
			// and leave the rest.

			var fatherInputs = father.InputNeurons;
			var motherInputs = mother.InputNeurons;

			for (int i = 0; i < InputNeuronCount; i++)
			{
				var outboundConnections = fatherInputs[i].OutboundConnections.Concat(
					motherInputs[i].OutboundConnections);

				// Remove any connections to neurons not in the child ant. By this point, the Neurons
				// list should be populated.
				outboundConnections = outboundConnections.Where(n => HiddenLayer.Neurons.Contains(n));

				// Wire up the input neurons.
				InputNeurons[i].OutboundConnections.AddRange(outboundConnections);
			}
		}
	}
}
