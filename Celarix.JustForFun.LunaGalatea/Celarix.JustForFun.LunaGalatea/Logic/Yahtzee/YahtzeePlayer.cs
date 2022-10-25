using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Logic.Yahtzee
{
    public static class YahtzeePlayer
    {
        private static readonly Random random;
        
        public static YahtzeeInfo Info { get; set; }
        
        public static int CurrentGameNumber { get; set; }

        public static int[] LastDiceRoll { get; set; }
        public static byte Holds { get; set; }
        public static int RollsLeft { get; set; }
        
        public static int? OnesScore { get; set; }
        public static int? TwosScore { get; set; }
        public static int? ThreesScore { get; set; }
        public static int? FoursScore { get; set; }
        public static int? FivesScore { get; set; }
        public static int? SixesScore { get; set; }
        public static int? ThreeOfAKindScore { get; set; }
        public static int? FourOfAKindScore { get; set; }
        public static int? FullHouseScore { get; set; }
        public static int? SmallStraightScore { get; set; }
        public static int? LargeStraightScore { get; set; }
        public static int? ChanceScore { get; set; }
        public static int? YahtzeeScore { get; set; }
        public static int YahtzeeBonusesScore { get; set; }

        public static int Subtotal =>
            OnesScore ?? 0
            + TwosScore ?? 0
            + ThreesScore ?? 0
            + FoursScore ?? 0
            + FivesScore ?? 0
            + SixesScore ?? 0;

        public static int Total =>
            Subtotal
            + ThreeOfAKindScore ?? 0
            + FourOfAKindScore ?? 0
            + FullHouseScore ?? 0
            + SmallStraightScore ?? 0
            + LargeStraightScore ?? 0
            + ChanceScore ?? 0
            + YahtzeeScore ?? 0
            + YahtzeeBonusesScore;

        static YahtzeePlayer()
        {
            random = new Random();
            LastDiceRoll = new int[5];
        }

        public static void Initialize(YahtzeeInfo info) => Info = info;

        public static void NextStep()
        {
            if (AllDiceZero())
            {
                // We're at the start of a new game, or we just recorded a score
                // on the game board. Roll the dice!
                RollDice();
            }
            else if (RollsLeft > 0)
            {
                // We have at least one dice roll and need to choose the best strategy
                // for maximizing score.
            }
        }

        private static bool AllDiceZero() =>
            LastDiceRoll[0] == 0
            && LastDiceRoll[1] == 0
            && LastDiceRoll[2] == 0
            && LastDiceRoll[3] == 0
            && LastDiceRoll[4] == 0;

        private static void RollDice()
        {
            for (int i = 0; i < 5; i++)
            {
                LastDiceRoll[i] = random.Next(1, 7);
            }

            RollsLeft -= 1;
        }

        private static bool GameOver() =>
            OnesScore.HasValue
            && TwosScore.HasValue
            && ThreesScore.HasValue
            && FoursScore.HasValue
            && FivesScore.HasValue
            && SixesScore.HasValue
            && ThreeOfAKindScore.HasValue
            && FourOfAKindScore.HasValue
            && FullHouseScore.HasValue
            && SmallStraightScore.HasValue
            && LargeStraightScore.HasValue
            && ChanceScore.HasValue
            && YahtzeeScore.HasValue;
    }

    internal sealed class YahtzeeStrategy
    {
        public int CurrentScore { get; set; }
        public int MaxScore { get; set; }
        public int RemainingDice { get; set; }
        public byte ResultingHold { get; set; }
    }
    
    internal static class YahtzeeStrategies
    {
        public static YahtzeeStrategy NumbersStrategy(int number, int[] dice)
        {
            byte hold = 0;
            if (dice[0] == number) { hold |= 0x10; }
            if (dice[1] == number) { hold |= 0x08; }
            if (dice[2] == number) { hold |= 0x04; }
            if (dice[3] == number) { hold |= 0x02; }
            if (dice[4] == number) { hold |= 0x01; }

            var count = dice.Count(d => d == number);
            return new YahtzeeStrategy
            {
                CurrentScore = count * number,
                MaxScore = number * 5,
                RemainingDice = 5 - count,
                ResultingHold = hold
            };
        }

        public static YahtzeeStrategy NOfAKindStrategy(int n, int[] dice)
        {
            var numberCounts = GetNumberCounts(dice);
            var bestHoldChoice = numberCounts.MaxBy(kvp => kvp.Value);

            byte hold = 0;
            if (dice[0] == bestHoldChoice.Key) { hold |= 0x10; }
            if (dice[1] == bestHoldChoice.Key) { hold |= 0x08; }
            if (dice[2] == bestHoldChoice.Key) { hold |= 0x04; }
            if (dice[3] == bestHoldChoice.Key) { hold |= 0x02; }
            if (dice[4] == bestHoldChoice.Key) { hold |= 0x01; }

            return new YahtzeeStrategy
            {
                CurrentScore = dice.Sum(),
                MaxScore = bestHoldChoice.Value * 5,
                RemainingDice = Math.Min(0, n - dice.Count(d => d == bestHoldChoice.Value)),
                ResultingHold = hold
            };
        }

        public static YahtzeeStrategy FullHouseStrategy(int[] dice)
        {
            var numberCounts = GetNumberCounts(dice);
            var topTwoHoldChoices = numberCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(2)
                .ToArray();

            var firstHoldChoice = topTwoHoldChoices.First();
            var secondHoldChoice = topTwoHoldChoices.Last();

            byte hold = 0;
            if (dice[0] == firstHoldChoice.Key || dice[0] == secondHoldChoice.Key) { hold |= 0x10; }
            if (dice[1] == firstHoldChoice.Key || dice[1] == secondHoldChoice.Key) { hold |= 0x08; }
            if (dice[2] == firstHoldChoice.Key || dice[2] == secondHoldChoice.Key) { hold |= 0x04; }
            if (dice[3] == firstHoldChoice.Key || dice[3] == secondHoldChoice.Key) { hold |= 0x02; }
            if (dice[4] == firstHoldChoice.Key || dice[4] == secondHoldChoice.Key) { hold |= 0x01; }

            return new YahtzeeStrategy
            {
                CurrentScore = firstHoldChoice.Value == 3 && secondHoldChoice.Value == 2
                    ? 25
                    : 0,
                MaxScore = 25,
                RemainingDice = (3 - firstHoldChoice.Value) + (2 - secondHoldChoice.Value),
                ResultingHold = hold
            };
        }

        public static YahtzeeStrategy StraightStrategy(int desiredLength, int scoreForStraight, int[] dice)
        {
            var sorted = dice
                .Select((d, i) => new KeyValuePair<int, int>(i, d))
                .OrderBy(kvp => kvp.Value)
                .ToArray();
            var ascendingStraightLengths = new[]
            {
                new KeyValuePair<int, int>(0, 0),
                new KeyValuePair<int, int>(1, 0),
                new KeyValuePair<int, int>(2, 0),
                new KeyValuePair<int, int>(3, 0),
                new KeyValuePair<int, int>(4, 0),
            };
            var ascendingDiceCount = 1;
            var lastSeenDie = int.MinValue;

            for (var i = 0; i < 5; i++)
            {
                var die = sorted[i].Value;
                ascendingDiceCount = die >= lastSeenDie
                    ? ascendingDiceCount + 1
                    : 1;
                ascendingStraightLengths[i] = new KeyValuePair<int, int>(ascendingStraightLengths[i].Key, ascendingDiceCount);
                lastSeenDie = die;
            }

            var longestStraightLength = ascendingStraightLengths.MaxBy(kvp => kvp.Value).Value;
            var endOfLongestStraight = Array.IndexOf(ascendingStraightLengths, longestStraightLength);
            var startOfLongestStraight = endOfLongestStraight - (longestStraightLength - 1);
            var holdIndices = ascendingStraightLengths
                .Skip(startOfLongestStraight)
                .Take((endOfLongestStraight - startOfLongestStraight) + 1)
                .Select(kvp => kvp.Key)
                .ToArray();

            byte hold = 0;
            foreach (var holdIndex in holdIndices)
            {
                hold |= (byte)(0x10 >> holdIndex);
            }

            return new YahtzeeStrategy
            {
                CurrentScore = longestStraightLength == desiredLength
                    ? scoreForStraight
                    : 0,
                MaxScore = scoreForStraight,
                RemainingDice = desiredLength - longestStraightLength,
                ResultingHold = hold,
            };
        }

        public static YahtzeeStrategy ChanceStrategy(int[] dice)
        {
            
        }

        private static Dictionary<int, int> GetNumberCounts(IEnumerable<int> dice)
        {
            var numberCounts = new Dictionary<int, int>();
            for (var i = 1; i <= 6; i++) { numberCounts.Add(i, 0); }

            foreach (var die in dice) { numberCounts[die] += 1; }

            return numberCounts;
        }
    }
}
