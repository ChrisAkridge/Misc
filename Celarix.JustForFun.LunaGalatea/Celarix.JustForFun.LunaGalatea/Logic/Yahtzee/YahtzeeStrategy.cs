using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;

internal sealed class YahtzeeStrategy
{
    public string ScoreName { get; set; }
    public int CurrentScore { get; set; }
    public int MaxScore { get; set; }
    public int RemainingDice { get; set; }
    public byte ResultingHold { get; set; }
}