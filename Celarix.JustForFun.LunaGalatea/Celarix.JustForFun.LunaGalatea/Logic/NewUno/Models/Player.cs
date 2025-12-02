using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.NewUno.Models
{
    internal sealed class Player
    {
        private Hand[] memoryHands;

        public Hand Hand { get; init; }

        public PlayAction Play(Game game, out Card? playedCard)
        {
            var currentDiscardCard = game.CurrentDiscardCard;
            var currentGameColor = game.CurrentColor;

            if (!currentDiscardCard.IsWildCard && currentGameColor != currentDiscardCard.Color)
            {
                throw new ArgumentException("Unless the current discard card is a wild card, the current card color must match the current discard card's color.");
            }

            var playableCards = GetPlayableCards(currentDiscardCard, currentGameColor);
            if (!playableCards.Any())
            {
                Hand.Add(game.Draw());
                playableCards = GetPlayableCards(currentDiscardCard, currentGameColor);
                if (!playableCards.Any())
                {
                    playedCard = null;
                    return PlayAction.DrawCardAndPass;
                }
            }

            // The game strategy can be pretty complex! So, let's handle the case of an emergency player,
            // (that is, a player with 3 or fewer cards) first. Each player remembers everyone's hands that
            // they've seen via swapping or rotation, and memory hands are how players track that.
            var emergencyPlayerIndices = GetEmergencyPlayerIndices(game);
            // So, if we have some emergency players, we want to adapt our strategy if we know all their cards
            // and that all their cards are the same color.
            if (emergencyPlayerIndices.Count > 0)
            {
                List<CardColor> safeColors = [CardColor.Red, CardColor.Green, CardColor.Yellow, CardColor.Blue];
                foreach (var playerIndex in emergencyPlayerIndices)
                {
                    var playerCardColor = GetPlayerCardColorIfKnownAndSame(playerIndex);
                    if (playerCardColor != null)
                    {
                        safeColors.Remove(playerCardColor.Value);
                    }
                }
            }

            // TODO: finish
            playedCard = null;
            return PlayAction.None;
        }

        private IReadOnlyList<Card> GetPlayableCards(Card currentDiscardCard, CardColor? currentGameColor)
        {
            var playableCards = new List<Card>();
            foreach (var card in Hand.Cards)
            {
                if (card.Type == CardType.Wild
                    || card.IsColorMatch(currentDiscardCard)
                    || card.IsNumberMatch(currentDiscardCard)
                    || card.IsSymbolMatch(currentDiscardCard)
                    || card.Color == currentGameColor)
                {
                    playableCards.Add(card);
                }
            }

            if (playableCards.Count == 0)
            {
                // We can always play "plain" Wilds but we can only play non-plains if there are no
                // other playable cards.
                return [..Hand.Cards.Where(c => c.IsWildCard)];
            }
            return playableCards;
        }

        private IReadOnlyList<int> GetEmergencyPlayerIndices(Game game)
        {
            var playerCardCounts = game.GetPlayerCardCounts();
            return [.. playerCardCounts
                .Where(kvp => kvp.Value <= 3)
                .Select(kvp => kvp.Key)];
        }

        private static PlayerTurnStrategy GetTurnStrategy(Game game)
        {
            var playerCardCounts = game.GetPlayerCardCounts();
            var emergencyPlayers = playerCardCounts.Where(kvp => kvp.Value <= 3).ToArray();

            if (emergencyPlayers.Length != 1)
            {
                return PlayerTurnStrategy.Default;
            }

            var emergencyPlayerIndex = emergencyPlayers.Single().Key;
            var nextPlayerInTurnOrder = game.NextPlayerIndexInTurnOrder();
            var nextPlayerInOppositeTurnOrder = game.NextPlayerIndexInOppositeTurnOrder();

            if (nextPlayerInOppositeTurnOrder == emergencyPlayerIndex)
            {
                return PlayerTurnStrategy.EmergencyPlayerTargetableWithReverse;
            }
            else if (nextPlayerInTurnOrder == emergencyPlayerIndex)
            {
                return PlayerTurnStrategy.Default;
            }
            else
            {
                return PlayerTurnStrategy.EmergencyPlayerNotTargetable;
            }
        }

        private static int GetCardValueForDefaultStrategy(Card card)
        {
            return card.Type switch
            {
                CardType.SkipEveryone => 12,
                CardType.WildDrawTen => 11,
                CardType.WildDrawSix => 10,
                CardType.WildDrawUntil => 9,
                CardType.WildReverseDrawFour => 8,
                CardType.DiscardAllOfColor => 7,
                CardType.DrawFour => 6,
                CardType.DrawTwo => 5,
                CardType.Reverse => 4,
                CardType.Skip => 3,
                CardType.Wild => 2,
                CardType.Number => 1,
                _ => throw new ArgumentException($"Unknown card type: {card.Type}"),
            };
        }

        private CardColor? GetPlayerCardColorIfKnownAndSame(int playerIndex)
        {
            var matchingMemoryHand = memoryHands
                .SingleOrDefault(mh => mh.BelongsToPlayerIndex == playerIndex);
            if (matchingMemoryHand != null
                && matchingMemoryHand.UnknownCardCount == 0)
            {
                var color = matchingMemoryHand.Cards[0].Color;
                foreach (var card in matchingMemoryHand.Cards.Skip(1))
                {
                    if (card.Color != color)
                    {
                        return null; // Not all cards are the same color
                    }
                }
                return color; // All cards are the same color
            }
            return null;
        }
    }
}
