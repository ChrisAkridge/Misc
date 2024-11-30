using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Uno
{
    internal sealed class EndlessGame
    {
        private const int DealAmount = 7;
        private const double PlayerCallsUnoOdds = 0.9d;
        private const double OthersNoticeMissingUnoCall = 0.75d;
        
        private readonly Player[] players;
        private readonly Random random = new Random();
        
        // Clockwise is positive, counterclockwise is negative, gameplay always starts clockwise to
        // the dealer's right, and the dealer is always player #1 (index 0).
        private int nextPlayerIndex = 1;
        private CardColor? wildChosenColor;
        
        public Card DiscardPileTop { get; set; }
        public PlayDirection PlayDirection { get; set; }

        public EndlessGame()
        {
            DrawPool.Fill();

            players = new Player[4];

            players[0] = new Player("James King");
            players[1] = new Player("Jack Ace");
            players[2] = new Player("Lily Diamond");
            players[3] = new Player("Dori Spade");

            foreach (var player in players)
            {
                // Deal the initial cards. Ordinarily, we do this one card at a time in clockwise order,
                // but since our deck is now infinitely deep, it's fine to just give everyone all their
                // cards at once.
                for (int j = 0; j < DealAmount; j++)
                {
                    player.GiveCard(Draw());
                }
            }
        }

        public void RunNextTurn()
        {
            var currentPlayer = players[nextPlayerIndex];
            var turnDecision = currentPlayer.PlayTurn(DiscardPileTop, wildChosenColor);
            if (turnDecision.WillDraw)
            {
                currentPlayer.GiveCard(Draw());
                turnDecision = currentPlayer.PlayTurn(DiscardPileTop, wildChosenColor);
                if (turnDecision.WillDraw)
                {
                    // Two draws in a row signals a pass.
                    AdvanceToNextPlayer();
                    return;
                }
            }

            var playedCard = turnDecision.PlayedCard!.Value;
            DiscardPileTop = playedCard;
            wildChosenColor = null;

            switch (currentPlayer.Hand.Count)
            {
                case 1:
                {
                    var playerCalledUno = random.NextDouble() <= PlayerCallsUnoOdds;
                    var othersNoticed = !playerCalledUno && random.NextDouble() <= OthersNoticeMissingUnoCall;
                    if (othersNoticed)
                    {
                        currentPlayer.GiveCards(Draw(), Draw());
                    }
                    break;
                }
                case 0:
                {
                    // You're winner!
                    currentPlayer.Wins += 1;
                    for (int i = 0; i < DealAmount; i++)
                    {
                        currentPlayer.GiveCard(Draw());
                    }
                    break;
                }
            }

            switch (playedCard.Symbol)
            {
                case null when playedCard.Color == CardColor.Wild:
                    wildChosenColor = currentPlayer.ChooseWildColor();
                    break;
                case CardSymbol.Reverse:
                    PlayDirection = PlayDirection == PlayDirection.Clockwise
                        ? PlayDirection.Counterclockwise
                        : PlayDirection.Clockwise;
                    AdvanceToNextPlayer();
                    break;
                case CardSymbol.Skip:
                    SkipNextPlayer();
                    break;
                case CardSymbol.DrawTwo:
                {
                    var drawingPlayer = players[GetNextPlayerIndex()];
                    drawingPlayer.GiveCards(Draw(), Draw());
                    SkipNextPlayer();
                    break;
                }
                case CardSymbol.WildDrawFour:
                {
                    var drawingPlayer = players[GetNextPlayerIndex()];
                    drawingPlayer.GiveCards(Draw(), Draw(), Draw(), Draw());
                    wildChosenColor = currentPlayer.ChooseWildColor();
                    SkipNextPlayer();
                    break;
                }
                default:
                    AdvanceToNextPlayer();
                    break;
            }
        }

        private int GetNextPlayerIndex() =>
            PlayDirection switch
            {
                PlayDirection.Clockwise when nextPlayerIndex < players.Length - 1 => nextPlayerIndex + 1,
                PlayDirection.Clockwise when nextPlayerIndex == players.Length - 1 => 0,
                PlayDirection.Counterclockwise when nextPlayerIndex > 0 => nextPlayerIndex - 1,
                PlayDirection.Counterclockwise when nextPlayerIndex == 0 => players.Length - 1,
                _ => throw new ArgumentOutOfRangeException(),
            };

        private void AdvanceToNextPlayer() => nextPlayerIndex = GetNextPlayerIndex();

        private void SkipNextPlayer()
        {
            AdvanceToNextPlayer();
            AdvanceToNextPlayer();
        }

        private Card Draw()
        {
            var poolIndex = random.Next(DrawPool.Cards.Count);
            return DrawPool.Cards[poolIndex];
        }
    }
}
