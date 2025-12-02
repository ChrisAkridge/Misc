using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NewUno.Models
{
    internal sealed class Hand
    {
        private readonly List<Card> cards = [];

        public int Index { get; init; }
        public bool IsMemoryHand { get; init; }
        public int BelongsToPlayerIndex { get; init; }
        public int UnknownCardCount { get; private set; }
        public IReadOnlyList<Card> Cards => cards.AsReadOnly();

        public void Add(Card card) => cards.Add(card);
        public void Remove(Card card) => cards.Remove(card);

        public Hand ToMemoryHand()
        {
            var memoryHand = new Hand
            {
                Index = Index,
                IsMemoryHand = true,
                UnknownCardCount = 0
            };

            foreach (var card in cards)
            {
                memoryHand.Add(new Card
                {
                    Color = card.Color,
                    Type = card.Type,
                    Number = card.Number
                });
            }

            return memoryHand;
        }

        public void DrewUnknownCard()
        {
            if (!IsMemoryHand)
            {
                throw new InvalidOperationException("This is your hand, not a memory hand.");
            }

            UnknownCardCount += 1;
        }

        public void PlayedCard(Card card)
        {
            if (!IsMemoryHand)
            {
                throw new InvalidOperationException("This is your hand, not a memory hand.");
            }

            if (!cards.Contains(card))
            {
                UnknownCardCount -= 1;
            }
            else
            {
                cards.Remove(card);
            }
        }
    }
}
