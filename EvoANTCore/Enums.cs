using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	public enum Direction
	{
		Up,
		Down,
		Left,
		Right
	}

	public enum NeuronType
	{
		Input,
		Hidden,
		Output
	}

	public enum GeneComparisonType
	{
		Matched,
		Disjoint,
		Excess
	}

	public enum MutationType
	{
		AddConnection,
		AddNeuron,
		RemoveConnection,
		RemoveNeuron
	}
}
