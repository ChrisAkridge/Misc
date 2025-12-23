using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Core;
using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;

namespace Celarix.JustForFun.FootballSimulator
{
    internal static class Extensions
    {
        public static int IndexOf<T>(this IEnumerable<T> sequence, T searchItem) where T : IEquatable<T>
        {
            var index = 0;

            foreach (var item in sequence)
            {
                if (item.Equals(searchItem))
                {
                    return index;
                }

                index += 1;
            }

            return -1;
        }

        // https://stackoverflow.com/a/69455054/2709212
        public static IList<T> Shuffle<T>(this IList<T> items, System.Random random)
        {
            for (int i = 0; i < items.Count - 1; i++)
            {
                int pos = random.Next(i, items.Count);
                (items[i], items[pos]) = (items[pos], items[i]);
            }

            return items;
        }

        public static Conference OtherConference(this Conference conference) =>
            conference switch
            {
                Conference.AFC => Conference.NFC,
                Conference.NFC => Conference.AFC,
                _ => throw new ArgumentOutOfRangeException(nameof(conference))
            };

        public static IEnumerable<T> RepeatEachItem<T>(this IEnumerable<T> sequence, int repeatCount)
        {
            foreach (var item in sequence)
            {
                for (int i = 0; i < repeatCount; i++)
                {
                    yield return item;
                }
            }
        }

        public static int PopulationCount(this BitArray bitArray)
        {
            int count = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray.Get(i))
                {
                    count++;
                }
            }
            return count;
        }

        public static double SampleNormalDistribution(this System.Random random, double mean, double standardDeviation)
        {
            if (random.NextDouble() < Constants.ChaosMultiplierChance)
            {
                Log.Information("Chaos multiplier triggered! Sampling from normal distribution with mean 0 and stddev {StdDev} * {Multiplier}", standardDeviation,
                    Constants.ChaosMultiplier);
                return Helpers.SampleNormalDistribution(0, standardDeviation * Constants.ChaosMultiplier, random);
            }

            var result = Helpers.SampleNormalDistribution(mean, standardDeviation, random);
            var resultStandardDeviationsFromMean = (result - mean) / standardDeviation;
            Log.Verbose("Sampled normal distribution (mean: {Mean}, stddev: {StdDev}) = {Result} ({StdDevsFromMean}σ)", mean, standardDeviation, result,
                resultStandardDeviationsFromMean.WithPlusSign());
            return result;
        }

        public static bool Chance(this System.Random random, double chance)
        {
            if (chance < 0 || chance > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), "Chance must be between 0 and 1.");
            }

            bool succeeds = random.NextDouble() < chance;
            Log.Verbose("Chance check for {Chance:P2} {Result}", chance, succeeds ? "succeeded" : "failed");
            return succeeds;
        }

        public static int Round(this double value) => (int)Math.Round(value);

        public static GameTeam Opponent(this GameTeam team)
        {
            return team switch
            {
                GameTeam.Home => GameTeam.Away,
                GameTeam.Away => GameTeam.Home,
                _ => throw new ArgumentOutOfRangeException(nameof(team))
            };
        }

        public static PossessionOnPlay ToPossessionOnPlay(this GameTeam team)
        {
            return team switch
            {
                GameTeam.Home => PossessionOnPlay.HomeTeamOnly,
                GameTeam.Away => PossessionOnPlay.AwayTeamOnly,
                _ => throw new ArgumentOutOfRangeException(nameof(team))
            };
        }

        public static string WithPlusSign(this double value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        public static string ToMinuteSecondString(this int totalSeconds)
        {
            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        public static string ToPeriodDisplayString(this int periodNumber)
        {
            return periodNumber switch
            {
                1 => "Q1",
                2 => "Q2",
                3 => "Q3",
                4 => "Q4",
                5 => "OT",
                >= 6 => $"{periodNumber - 4}OT",
                _ => throw new ArgumentOutOfRangeException(nameof(periodNumber))
            };
        }
    }
}
