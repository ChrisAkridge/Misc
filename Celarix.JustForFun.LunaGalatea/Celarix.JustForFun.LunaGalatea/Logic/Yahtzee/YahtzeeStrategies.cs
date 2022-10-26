using System;
using System.Collections.Generic;
using System.Linq;

namespace Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;

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
            ScoreName = number switch
            {
                1 => "Ones",
                2 => "Twos",
                3 => "Threes",
                4 => "Fours",
                5 => "Fives",
                6 => "Sixes",
                _ => throw new ArgumentOutOfRangeException()
            },
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
            ScoreName = (n == 5)
                ? "Yahtzee"
                : $"{n} of a Kind",
            CurrentScore = dice.Sum(),
            MaxScore = bestHoldChoice.Key * 5,
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
            ScoreName = "Full House",
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
        var ascendingDiceCount = 0;
        var lastSeenDie = 0;

        for (var i = 0; i < 5; i++)
        {
            var die = sorted[i].Value;
            ascendingDiceCount = die == lastSeenDie + 1
                ? ascendingDiceCount + 1
                : 1;
            ascendingStraightLengths[i] = new KeyValuePair<int, int>(ascendingStraightLengths[i].Key, ascendingDiceCount);
            lastSeenDie = die;
        }

        var longestStraightLength = ascendingStraightLengths.MaxBy(kvp => kvp.Value).Value;
        var endOfLongestStraight = ascendingStraightLengths.IndexOf(kvp => kvp.Value == longestStraightLength);
        var startOfLongestStraight = endOfLongestStraight - (longestStraightLength - 1);
        // wrong: uses indices from ascendingStraightLengths, not the original indices from the incoming roll
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
            ScoreName = desiredLength switch
            {
                4 => "Small Straight",
                5 => "Large Straight",
                _ => throw new ArgumentOutOfRangeException()
            },
            CurrentScore = longestStraightLength == desiredLength
                ? scoreForStraight
                : 0,
            MaxScore = scoreForStraight,
            RemainingDice = desiredLength - longestStraightLength,
            ResultingHold = hold,
        };
    }

    public static YahtzeeStrategy ChanceStrategy(int[] dice) =>
        new YahtzeeStrategy
        {
            ScoreName = "Chance",
            CurrentScore = dice.Sum(),
            MaxScore = 30,
            RemainingDice = 0,
            ResultingHold = 0
        };

    private static Dictionary<int, int> GetNumberCounts(IEnumerable<int> dice)
    {
        var numberCounts = new Dictionary<int, int>();
        for (var i = 1; i <= 6; i++) { numberCounts.Add(i, 0); }

        foreach (var die in dice) { numberCounts[die] += 1; }

        return numberCounts;
    }
}