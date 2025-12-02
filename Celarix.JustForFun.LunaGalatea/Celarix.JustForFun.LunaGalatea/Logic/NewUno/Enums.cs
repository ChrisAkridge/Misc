using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NewUno
{
    internal enum CardColor
    {
        Red,
        Green,
        Yellow,
        Blue,
        Wild
    }

    [Flags]
    internal enum CardActions
    {
        None = 0x0,
        RotateHands = 0x1,
        SwapHands = 0x2,
    }

    internal enum CardType
    {
        Number,
        Skip,
        Reverse,
        DrawTwo,
        DrawFour,
        DiscardAllOfColor,
        SkipEveryone,
        Wild,
        WildDrawFour,
        WildReverseDrawFour,
        WildDrawSix,
        WildDrawTen,
        WildDrawUntil
    }

    internal enum PlayDirection
    {
        Clockwise,
        CounterClockwise
    }

    internal enum PlayAction
    {
        None,
        PlayCard,
        DrawCardAndPass
    }

    internal enum PlayerTurnStrategy
    {
        Default,
        EmergencyPlayerTargetableWithReverse,
        EmergencyPlayerNotTargetable
    }
}
