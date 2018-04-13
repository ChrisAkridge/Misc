using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoANTCore
{
	internal interface IGene
	{
		int InnovationNumber { get; set; }

		IGene Clone();
	}
}
