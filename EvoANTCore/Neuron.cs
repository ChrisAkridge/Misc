using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public sealed class Neuron
	{
		private static Random neuronCreatorRNG = new Random();
		private double currentInputStrength;	// The sum of all the firing strengths of any inbound neurons.

		public List<Neuron> OutboundConnections { get; private set; } = new List<Neuron>();

		public double FireStrength { get; private set; }
		public double FireRequirementBaseline { get; private set; }
		public double CurrentFireRequirement { get; private set; }
		public double FireFatigueIncrease { get; private set; }
		public double FireFatigueDecrease { get; private set; }

		public string SpecialName { get; private set; }

		public Action OnFire { get; set; }
		public bool IsOutputNeuron => OnFire != null;

		public bool IsFiring { get; internal set; }

		private Neuron() { }

		internal Neuron(double fireStrength, double fireRequirementBaseline, double fatigueIncrease,
			double fatigueDecrease, string specialName = null)
		{
			FireStrength = fireStrength;
			FireRequirementBaseline = fireRequirementBaseline;
			CurrentFireRequirement = FireRequirementBaseline;
			FireFatigueIncrease = fatigueIncrease;
			FireFatigueDecrease = fatigueDecrease;
			SpecialName = specialName;
		}

		/// <summary>
		/// When a neuron fires, it calls this method on all outbound neurons.
		/// </summary>
		/// <param name="neuronStrength">The strength of the neuron that fired.</param>
		public void InboundNeuronFired(double neuronStrength)
		{
			// If neuron A is connected to neuron B, and B is also connected to A, it's possible to
			// have a scenario where A triggers B triggers A and so forth. Not quite an infinite
			// loop because of fatigue, but still undesirable. Therefore, a neuron can only fire
			// once per timestep.
			if (IsFiring) { return; }

			currentInputStrength += neuronStrength;
			if (currentInputStrength >= CurrentFireRequirement) { Fire(); }
		}

		public void Fire()
		{
			IsFiring = true;

			// Signal all outbound neurons that this neuron fired.
			foreach (var neuron in OutboundConnections)
			{
				neuron.InboundNeuronFired(FireStrength);
			}

			// Run the on fire action, if any.
			OnFire?.Invoke();

			// Increase the fatigue (the firing strength required to trigger this neuron again).
			CurrentFireRequirement += FireFatigueIncrease;
		}

		public void ClearAfterTimestep()
		{
			IsFiring = false;
			currentInputStrength = 0d;
			CurrentFireRequirement -= FireFatigueDecrease;
			if (CurrentFireRequirement < FireRequirementBaseline)
			{
				CurrentFireRequirement = FireRequirementBaseline;
			}
		}

		public static Neuron CreateRandom(WorldSettings settings, Action onFire = null)
		{
			var neuron = new Neuron();

			neuron.FireStrength = neuronCreatorRNG.NextDouble() * settings.MaxRandomNeuronFireStrength;
			neuron.FireRequirementBaseline = neuronCreatorRNG.NextDouble() * settings.MaxRandomNeuronFireRequirementBaseline;
			neuron.CurrentFireRequirement = neuron.FireRequirementBaseline;
			neuron.FireFatigueIncrease = neuronCreatorRNG.NextDouble() * settings.MaxRandomNeuronFireFatigueIncrease;
			neuron.FireFatigueDecrease = neuronCreatorRNG.NextDouble() * settings.MaxRandomNeuronFireFatigueDecrease;

			neuron.OnFire = null;

			return neuron;
		}
	}
}
