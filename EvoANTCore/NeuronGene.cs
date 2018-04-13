using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal sealed class NeuronGene : IGene
	{
		public int InnovationNumber { get; set; }
		public int Index { get; internal set; }
		public NeuronType Type { get; internal set; }
		public string SpecialName { get; internal set; }
		public Action OnFire { get; internal set; }

		public double FiringStrength { get; internal set; }
		public double FiringRequirement { get; internal set; }
		public double FatigueIncrease { get; internal set; }
		public double FatigueDecrease { get; internal set; }

		public IGene Clone()
		{
			var result = new NeuronGene();

			result.Index = Index;
			result.Type = Type;
			result.SpecialName = SpecialName;
			result.OnFire = null;

			result.FiringStrength = FiringStrength;
			result.FiringRequirement = FiringRequirement;
			result.FatigueIncrease = FatigueIncrease;
			result.FatigueDecrease = FatigueDecrease;

			return result;
		}
	}
}
