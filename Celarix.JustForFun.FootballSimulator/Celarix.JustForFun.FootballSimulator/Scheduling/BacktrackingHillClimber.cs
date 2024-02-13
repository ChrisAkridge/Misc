using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Collections;

namespace Celarix.JustForFun.FootballSimulator.Scheduling
{
    internal sealed class BacktrackingHillClimber<T>
    {
        private struct SwapEvent
        {
            public int FirstIndex { get; set; }
            public int SecondIndex { get; set; }
        }

        private const int MaxShuffleHistory = 10;

        private readonly IList<T> list;
        private readonly Func<T, int, int> itemScorer;
        private readonly Stack<SwapEvent> swapHistory;
        private readonly Random random;

        public BacktrackingHillClimber(IList<T> list, Func<T, int, int> itemScorer, Random random)
        {
            this.list = list;
            this.itemScorer = itemScorer;
            swapHistory = new Stack<SwapEvent>();
            this.random = random;
        }

        public void RunClimbStep()
        {
            // A poor man's hill climbing algorithm. Remember swaps that make the score worse until
            // we fill the stack, then backtrack a random amount of steps and try from there.
            var badItemIndices = new List<int>();
            var totalScore = 0;

            for (int i = 0; i < list.Count; i++)
            {
                T? item = list[i];
                var score = itemScorer(item, i);
                if (score < 0) { badItemIndices.Add(i); }
                totalScore += score;
            }

            if (totalScore == 0) { return; }

            if (badItemIndices.Count == list.Count)
            {
                throw new InvalidOperationException("Should be unreachable, but good to guard against infinite loops.");
            }

            if (swapHistory.Count == MaxShuffleHistory)
            {
                int backTrackCount = random.Next(1, MaxShuffleHistory);
                for (int i = 0; i < backTrackCount; i++)
                {
                    var lastEvent = swapHistory.Pop();
                    (list[lastEvent.FirstIndex], list[lastEvent.SecondIndex]) = (list[lastEvent.SecondIndex], list[lastEvent.FirstIndex]);
                }

                Console.WriteLine($"Backtracking {backTrackCount} steps, back to score {GetTotalScore()}");
                return;
            }

            int badItemIndexToSwap = random.Next(badItemIndices.Count);
            int goodItemIndexToSwap;

            do
            {
                goodItemIndexToSwap = random.Next(list.Count);
            } while (badItemIndices.Contains(goodItemIndexToSwap));

            var swap = new SwapEvent
            {
                FirstIndex = badItemIndices[badItemIndexToSwap],
                SecondIndex = goodItemIndexToSwap
            };
            (list[swap.FirstIndex], list[swap.SecondIndex]) = (list[swap.SecondIndex], list[swap.FirstIndex]);

            var newScore = GetTotalScore();
            Console.WriteLine($"{totalScore} to {newScore}, stack depth {swapHistory.Count}");
            if (newScore < totalScore)
            {
                swapHistory.Push(swap);
            }
        }

        private int GetTotalScore()
        {
            var score = 0;

            for (int i = 0; i < list.Count; i++)
            {
                T? item = list[i];
                score += itemScorer(item, i);
            }

            return score;
        }
    }
}
