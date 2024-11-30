using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Uno
{
    internal sealed class TurnDecision
    {
        public Card? PlayedCard { get; set; }
        public bool WillDraw => PlayedCard == null;
    }
}
