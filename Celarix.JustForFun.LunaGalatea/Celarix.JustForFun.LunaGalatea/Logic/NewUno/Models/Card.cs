using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NewUno.Models
{
    internal sealed class Card
    {
        public CardColor Color { get; init; }
        public CardType Type { get; init; }
        public int? Number { get; init; }

        public bool IsWildCard => Type switch
        {
            CardType.Wild => true,
            CardType.WildDrawFour => true,
            CardType.WildReverseDrawFour => true,
            CardType.WildDrawSix => true,
            CardType.WildDrawTen => true,
            CardType.WildDrawUntil => true,
            _ => false
        };

        public bool IsColorMatch(Card other)
        {
            return Color != CardColor.Wild
                && other.Color != CardColor.Wild
                && Color == other.Color;
        }

        public bool IsNumberMatch(Card other)
        {
            return Number.HasValue && other.Number.HasValue
                && (Number.Value == other.Number.Value);
        }

        public bool IsSymbolMatch(Card other)
        {
            return Type != CardType.Number && other.Type != CardType.Number
                && !IsWildCard && !other.IsWildCard
                && (Type == other.Type);
        }
    }
}
