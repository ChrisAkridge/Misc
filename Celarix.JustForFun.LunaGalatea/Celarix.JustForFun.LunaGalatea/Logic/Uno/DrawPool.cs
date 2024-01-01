using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Uno
{
    internal static class DrawPool
    {
        private static Card[] cards = Array.Empty<Card>();
        public static IReadOnlyList<Card> Cards => cards;

        public static void Fill()
        {
            var numberRange = Enumerable.Range(0, 10);
            var symbolRange = new[]
            {
                CardSymbol.DrawTwo,
                CardSymbol.Reverse,
                CardSymbol.Skip
            };
            var colorRange = new[]
            {
                CardColor.Red,
                CardColor.Green,
                CardColor.Blue,
                CardColor.Yellow
            };

            // https://stackoverflow.com/a/20870526
            // firstList.Join(secondList, x => true, y => true, (m, n) => new { m, n });
            var numberCards = RepeatEachItem(numberRange, 2)
                .Join(colorRange, _ => true, _ => true, (n, c) => new { n, c })
                .Select(nc => new Card
                {
                    Number = nc.n,
                    Color = nc.c
                });
            var symbolCards = RepeatEachItem(symbolRange, 2)
                .Join(colorRange, _ => true, _ => true, (s, c) => new { s, c })
                .Select(sc => new Card
                {
                    Symbol = sc.s,
                    Color = sc.c
                });

            cards = numberCards
                .Concat(symbolCards)
                .Concat(Enumerable.Repeat(new Card
                {
                    Color = CardColor.Wild
                }, 4))
                .Concat(Enumerable.Repeat(new Card
                {
                    Color = CardColor.Wild,
                    Symbol = CardSymbol.WildDrawFour
                }, 4))
                .ToArray();
        }

        private static IEnumerable<T> RepeatEachItem<T>(IEnumerable<T> sequence, int times)
        {
            foreach (T item in sequence)
            {
                for (int i = 0; i < times; i++)
                {
                    yield return item;
                }
            }
        }
    }
}
