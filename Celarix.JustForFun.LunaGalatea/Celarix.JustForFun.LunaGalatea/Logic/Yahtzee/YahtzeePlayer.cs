using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static event EventHandler<YahtzeeInfo> GameOver; 

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
                var bestStrategy = GetStrategiesByQuality().First();

                if (bestStrategy.RemainingDice == 0 && TryRecordScore(bestStrategy))
                {
                    return;
                }

                Holds = bestStrategy.ResultingHold;
                RollDice();
            }
            else if (!IsGameOver())
            {
                // Time to choose a score.
                var strategies = GetStrategiesByQuality();

                foreach (var strategy in strategies)
                {
                    if (TryRecordScore(strategy))
                    {
                        break;
                    }
                }
            }
            else
            {
                // Finish the game!
                OnGameOver(new YahtzeeInfo
                {
                    TotalGamesPlayed = 1,
                    TotalPointsScored = Total,
                    TotalYahtzeeCount = (YahtzeeScore != null ? 1 : 0) + (YahtzeeBonusesScore / 100)
                });
                
                ResetGame();
            }
        }

        private static void ResetDice()
        {
            LastDiceRoll[0] = LastDiceRoll[1] = LastDiceRoll[2]
                = LastDiceRoll[3] = LastDiceRoll[4] = 0;
            Holds = 0x00;
            RollsLeft = 3;
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
                if ((Holds & (0b10000 >> i)) != 0)
                {
                    LastDiceRoll[i] = random.Next(1, 7);
                }
            }

            RollsLeft -= 1;
        }

        private static bool IsGameOver() =>
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

        private static List<YahtzeeStrategy> GetStrategiesByQuality()
        {
            var strategies = new List<YahtzeeStrategy>();

            for (var i = 1; i <= 6; i++)
            {
                strategies.Add(YahtzeeStrategies.NumbersStrategy(i, LastDiceRoll));
            }
            
            strategies.Add(YahtzeeStrategies.NOfAKindStrategy(3, LastDiceRoll));
            strategies.Add(YahtzeeStrategies.NOfAKindStrategy(4, LastDiceRoll));
            strategies.Add(YahtzeeStrategies.FullHouseStrategy(LastDiceRoll));
            strategies.Add(YahtzeeStrategies.StraightStrategy(3, 30, LastDiceRoll));
            strategies.Add(YahtzeeStrategies.StraightStrategy(4, 40, LastDiceRoll));
            strategies.Add(YahtzeeStrategies.ChanceStrategy(LastDiceRoll));
            strategies.Add(YahtzeeStrategies.NOfAKindStrategy(5, LastDiceRoll));

            return strategies
                .OrderByDescending(s => (double)s.CurrentScore / s.MaxScore)
                .ThenByDescending(s => s.RemainingDice)
                .ToList();
        }

        private static bool TryRecordScore(YahtzeeStrategy strategy)
        {
            if (!CanRecordScore(strategy.ScoreName))
            {
                return false;
            }

            Action<int> setScoreAction = strategy.ScoreName switch
            {
                "Ones" => s => OnesScore = s,
                "Twos" => s => TwosScore = s,
                "Threes" => s => ThreesScore = s,
                "Fours" => s => FoursScore = s,
                "Fives" => s => FivesScore = s,
                "Sixes" => s => SixesScore = s,
                "3 of a Kind" => s => ThreeOfAKindScore = s,
                "4 of a Kind" => s => FourOfAKindScore = s,
                "Full House" => s => FullHouseScore = s,
                "Small Straight" => s => SmallStraightScore = s,
                "Large Straight" => s => LargeStraightScore = s,
                "Chance" => s => ChanceScore = s,
                "Yahtzee" => s => YahtzeeScore = s,
                _ => throw new ArgumentOutOfRangeException()
            };

            setScoreAction(strategy.CurrentScore);
            ResetDice();

            if (LastDiceRoll.All(i => i == LastDiceRoll[0]) && YahtzeeScore != null)
            {
                // Bonus!
                YahtzeeBonusesScore += 100;
            }

            return true;
        }

        private static bool CanRecordScore(string scoreName)
        {
            return scoreName switch
            {
                "Ones" => OnesScore == null,
                "Twos" => TwosScore == null,
                "Threes" => ThreesScore == null,
                "Fours" => FoursScore == null,
                "Fives" => FivesScore == null,
                "Sixes" => SixesScore == null,
                "3 of a Kind" => ThreeOfAKindScore == null,
                "4 of a Kind" => FourOfAKindScore == null,
                "Full House" => FullHouseScore == null,
                "Small Straight" => SmallStraightScore == null,
                "Large Straight" => LargeStraightScore == null,
                "Chance" => ChanceScore == null,
                "Yahtzee" => YahtzeeScore == null,
                _ => false
            };
        }

        private static void OnGameOver(YahtzeeInfo info) => GameOver?.Invoke(null, info);

        private static void ResetGame()
        {
            CurrentGameNumber += 1;
            ResetDice();

            OnesScore = TwosScore = ThreesScore = FoursScore = FivesScore = SixesScore = ThreeOfAKindScore =
                FourOfAKindScore = FullHouseScore =
                    SmallStraightScore = LargeStraightScore = ChanceScore = YahtzeeScore = null;
            YahtzeeBonusesScore = 0;
        }
    }
}
