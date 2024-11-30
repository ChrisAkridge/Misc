using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Uno
{
    internal sealed class Player
    {
        private List<Card> hand = new List<Card>();
        
        public string Name { get; set; }
        public int Wins { get; set; }
        public IReadOnlyList<Card> Hand => hand;

        public Player(string name) => Name = name;

        public void GiveCard(Card card) => hand.Add(card);
        
        public void GiveCards(params Card[] cards) => hand.AddRange(cards);

        public TurnDecision PlayTurn(Card discardPileTop, CardColor? wildChosenColor)
        {
            throw new NotImplementedException();
        }

        public CardColor ChooseWildColor()
        {
            throw new NotImplementedException();
        }
    }
}
