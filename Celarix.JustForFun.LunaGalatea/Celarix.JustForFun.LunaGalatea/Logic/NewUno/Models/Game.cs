using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NewUno.Models
{
    internal sealed class Game
    {
        private Player[] players;
        private int nextPlayerIndex;
        private readonly Card[] standardDeck = BuildStandardDeck();
        private Random random = new Random();

        public int GameNumber { get; init; }
        public DateTimeOffset StartTime { get; init; }
        public DateTimeOffset? EndTime { get; init; }

        public Card CurrentDiscardCard { get; private set; }
        public PlayDirection CurrentDirection { get; private set; }
        public CardColor CurrentColor { get; private set; }

        public Card Draw()
        {
            var index = random.Next(standardDeck.Length);
            return standardDeck[index];
        }

        public IReadOnlyDictionary<int, int> GetPlayerCardCounts()
        {
            var playerCards = new Dictionary<int, int>();
            for (int i = 0; i < players.Length; i++)
            {
                playerCards[i] = players[i].Hand.Cards.Count;
            }
            return playerCards;
        }

        public int NextPlayerIndexInTurnOrder()
        {
            if (CurrentDirection == PlayDirection.Clockwise)
            {
                return NextPlayerInClockwiseOrder();
            }
            else if (CurrentDirection == PlayDirection.CounterClockwise)
            {
                return NextPlayerInCounterClockwiseOrder();
            }
            else
            {
                throw new InvalidOperationException("Invalid play direction.");
            }
        }

        public int NextPlayerIndexInOppositeTurnOrder()
        {
            if (CurrentDirection == PlayDirection.Clockwise)
            {
                return NextPlayerInCounterClockwiseOrder();
            }
            else if (CurrentDirection == PlayDirection.CounterClockwise)
            {
                return NextPlayerInClockwiseOrder();
            }
            else
            {
                throw new InvalidOperationException("Invalid play direction.");
            }
        }

        private static Card[] BuildStandardDeck()
        {
            // This version of Uno simulates an infinite deck by returning a card over a distribution
            // of the typical Show Em' No Mercy deck. There are:
            // - 80 number cards (0-9 of each color, twice, 10 * 40 * 2 = 80)
            // - 12 "discard all of color" cards (3 of each color, 3 * 4 = 12)
            // - 12 "skip" cards (3 of each color, 3 * 4 = 12)
            // - 12 "reverse" cards (3 of each color, 3 * 4 = 12)
            // - 8 "draw 2" cards (2 of each color, 2 * 4 = 8)
            // - 8 "draw 4" cards (2 of each color, 2 * 4 = 8)
            // - 8 "skip everyone" cards (2 of each color, 2 * 4 = 8)
            // - 8 "wild draw until" cards
            // - 8 "wild reverse draw 4" cards
            // - 4 "wild draw 6" cards
            // - 4 "wild draw 10" cards
            // We also add some cards from base Uno:
            // - 8 "wild" cards

            CardColor[] cardColors = [CardColor.Red, CardColor.Green, CardColor.Yellow, CardColor.Blue];
            return cardColors
            .SelectMany(color => Enumerable.Range(0, 10)
            .SelectMany(i => Enumerable.Range(0, 2)
            .Select(_ => MakeCard(color, CardType.Number, i))))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.DiscardAllOfColor))
                .Repeat(3))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.Skip))
                .Repeat(3))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.Reverse))
                .Repeat(3))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.DrawTwo))
                .Repeat(2))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.DrawFour))
                .Repeat(2))
            .Concat(cardColors
                .Select(c => MakeCard(c, CardType.SkipEveryone))
                .Repeat(2))
            .Concat(Enumerable.Repeat(MakeCard(CardColor.Wild, CardType.WildDrawUntil), 8))
            .Concat(Enumerable.Repeat(MakeCard(CardColor.Wild, CardType.WildReverseDrawFour), 8))
            .Concat(Enumerable.Repeat(MakeCard(CardColor.Wild, CardType.WildDrawSix), 4))
            .Concat(Enumerable.Repeat(MakeCard(CardColor.Wild, CardType.WildDrawTen), 4))
            .Concat(Enumerable.Repeat(MakeCard(CardColor.Wild, CardType.Wild), 8))
            .ToArray();
        }

        private static Card MakeCard(CardColor color, CardType type, int? number = null)
        {
            return new Card
            {
                Color = color,
                Type = type,
                Number = number
            };
        }

        private int NextPlayerInClockwiseOrder() =>
            (nextPlayerIndex + 1) % players.Length;
        private int NextPlayerInCounterClockwiseOrder() =>
            (nextPlayerIndex - 1 + players.Length) % players.Length;
    }
}
