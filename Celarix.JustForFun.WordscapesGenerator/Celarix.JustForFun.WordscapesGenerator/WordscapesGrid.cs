using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.WordscapesGenerator
{
    public sealed class WordscapesGrid
    {
        private readonly Random random = new Random();
        
        private readonly SparseGrid grid = new SparseGrid();
        private readonly List<string> unplacedWords;
        private readonly Dictionary<char, List<Point>> letterSeeds = new Dictionary<char, List<Point>>();
        private readonly Stack<Backtrack> backtracks = new Stack<Backtrack>();

        public bool PlacementComplete => !unplacedWords.Any();

        public string NextWord => unplacedWords.LastOrDefault()!;
        
        public string LetterPalette { get; init; }

        public WordscapesGrid(IEnumerable<string> words)
        {
            for (char c = ' '; c <= (char)0x7F; c++)
            {
                letterSeeds.Add(c, new List<Point>());
            }

            unplacedWords = new List<string>(words);
            LetterPalette = GetLetterPalette();

            var firstWord = PopWord();
            //TryPlaceWord(firstWord, new Point(0, 0), random.NextDouble() < 0.5d
            //    ? WordPlaceDirection.Down
            //    : WordPlaceDirection.Right, out _);
            TryPlaceWord(firstWord, new Point(0, 0), WordPlaceDirection.Down, out _);
        }

        public void PlaceAllWords()
        {
            var placedWordCount = 0;
            
            while (unplacedWords.Any())
            {
                PlaceNextWord();
                placedWordCount += 1;
                if (placedWordCount % 100 == 0)
                {
                    Console.WriteLine($"Placed {placedWordCount} words");
                }
            }
        }

        public void PlaceNextWord()
        {
            var word = PopWord();
            var possibleSeeds = word
                .Select((l, i) => new
                {
                    Letter = l,
                    Index = i,
                    Seeds = letterSeeds[l]
                })
                .Where(ls => ls.Seeds.Any())
                .ToArray();

            foreach (var seed in possibleSeeds)
            {
                foreach (var location in seed.Seeds)
                {
                    var wordDirection = (grid[location.X, location.Y] & 0x80) == 0
                        ? WordPlaceDirection.Right
                        : WordPlaceDirection.Down;
                    var wordStartLocation = wordDirection == WordPlaceDirection.Down
                        ? new Point(location.X, location.Y - seed.Index)
                        : new Point(location.X - seed.Index, location.Y);
                
                    if (TryPlaceWord(word, wordStartLocation, wordDirection, out var removedSeeds))
                    {
                        foreach (var removedSeed in removedSeeds)
                        {
                            var cellCharacter = (char)(grid[removedSeed.X, removedSeed.Y] & 0x7F);
                            if (cellCharacter == '\0') { continue; }
                        
                            letterSeeds[cellCharacter].Remove(removedSeed);
                        }

                        return;
                    }
                }
            }
            
            //// We couldn't place this word at any seed for this letter.
            //// Yes, we could try other letters, but, eh.
            //// Just backtrack.
            //var backtrack = backtracks.Pop();
            //UndoWordPlacement(backtrack);
            PushWordAsLast(word);
        }

        public string[] GetGridText() => grid.GetPrintableLines();

        private string GetLetterPalette()
        {
            var characterCounts = new int[0x80 - 0x20];

            for (char c = ' '; c <= 0x7F; c++)
            {
                var arrayIndex = c - 0x20;
                var countInWordWithMostOfCharacter = unplacedWords
                    .Select(w => w.Count(wc => wc == c))
                    .OrderByDescending(count => count)
                    .FirstOrDefault();
                characterCounts[arrayIndex] = countInWordWithMostOfCharacter;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < characterCounts.Length; i++)
            {
                builder.Append(new string((char)(i + 0x20), characterCounts[i]));
            }

            return builder.ToString();
        }
        
        private bool TryPlaceWord(string word,
            Point location,
            WordPlaceDirection direction,
            out List<Point> removedSeeds)
        {
            var delta = GetDelta(direction);
            var letterPlacements = new List<Point>();
            var currentLocation = location;

            removedSeeds = new List<Point>();

            if (!CanPlaceFirstLetter(word[0], location, direction)) { return false; }

            for (var i = 0; i < word.Length; i++)
            {
                var letter = word[i];
                var nextLetterInWord = i < word.Length - 1
                    ? word[i + 1]
                    : (char?)null;
                var asciiLetter = (byte)(letter & 0x7F);
                var canPlaceLetter = CanPlaceLetter(letter, nextLetterInWord, currentLocation, direction);

                switch (canPlaceLetter)
                {
                    case CanPlaceLetterResult.DesiredLetterAlreadyPresent:
                        // Skip over this letter since it's already on the grid...
                        // but do remove the seed we just went over. Also, remove
                        // the letter we're overwriting from the seeds and the ones
                        // next and previous in the other direction.
                        removedSeeds.Add(currentLocation + -delta.Swap());
                        removedSeeds.Add(currentLocation);
                        removedSeeds.Add(currentLocation + delta.Swap());
                        break;
                    case CanPlaceLetterResult.Yes:
                        // Store which direction the word is in by the high bit of the
                        // letter. It's ugly and Joel Spolsky might yell at me, but at
                        // least I don't need another grid.
                        var directionMask = direction == WordPlaceDirection.Down
                            ? 0x00
                            : 0x80;
                        grid[currentLocation.X, currentLocation.Y] = (byte)(directionMask | asciiLetter);
                        letterPlacements.Add(currentLocation);
                        break;
                    case CanPlaceLetterResult.No:
                    {
                            // Word cannot be placed; backtrack and undo existing placements.
                            foreach (var placement in letterPlacements)
                            {
                                grid[placement.X, placement.Y] = 0;
                            }
                            
                            return false;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                currentLocation += delta;
            }

            backtracks.Push(new Backtrack(word, letterPlacements, removedSeeds));
            SetNewSeeds(location, word.Length, direction);
            return true;
        }
        
        private CanPlaceLetterResult CanPlaceLetter(char letter, char? nextLetterInWord, Point location, WordPlaceDirection direction)
        {
            var currentCellValue = grid[location.X, location.Y];
            var cellEmpty = currentCellValue == 0;
            var cellHasDesiredLetter = (byte)(currentCellValue & 0x7F) == (byte)(letter & 0x7F);
            var nextLetter = (char)((direction switch
                {
                    WordPlaceDirection.Down => grid[location.X, location.Y + 1],
                    WordPlaceDirection.Right => grid[location.X + 1, location.Y],
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                })
                & 0x7F);
            var nextLetterEmpty = nextLetter == 0;
            var crossCellAboveLocation = location + GetDelta(direction).Swap();
            var crossCellBelowLocation = location + -(GetDelta(direction).Swap());
            var crossCellAboveEmpty = grid[crossCellAboveLocation.X, crossCellAboveLocation.Y] == 0;
            var crossCellBelowEmpty = grid[crossCellBelowLocation.X, crossCellBelowLocation.Y] == 0;

            if (cellHasDesiredLetter) { return CanPlaceLetterResult.DesiredLetterAlreadyPresent; }

            if (cellEmpty)
            {
                if (nextLetterEmpty || nextLetter == nextLetterInWord)
                {
                    if (crossCellAboveEmpty && crossCellBelowEmpty)
                    {
                        return CanPlaceLetterResult.Yes;
                    }
                }
            }

            return CanPlaceLetterResult.No;
        }

        private bool CanPlaceFirstLetter(char letter, Point location, WordPlaceDirection direction)
        {
            var reverseDelta = -GetDelta(direction);
            var previousCellLocation = location + reverseDelta;
            return grid[previousCellLocation.X, previousCellLocation.Y] == 0;
        }

        private void SetNewSeeds(Point initialWordLocation, int wordLength, WordPlaceDirection direction)
        {
            // For right-directional words, a cell is a seed if:
            // - The six cells above and below it are all empty (# in the grid below):
            // ### 
            //  W
            // ###
            
            // For down-directional words, a cell is a seed if:
            // - The six cells to its left and right are all empty (# in the grid below):
            // # #
            // #W#
            // # #
            
            var delta = GetDelta(direction);
            var currentLocation = initialWordLocation;
            
            for (var i = 0; i < wordLength; i++)
            {
                var cellsToCheck = direction == WordPlaceDirection.Right
                    ? new[]
                    {
                        currentLocation + new Point(-1, -1),
                        currentLocation + new Point(0, -1),
                        currentLocation + new Point(1, -1),
                        currentLocation + new Point(-1, 1),
                        currentLocation + new Point(0, 1),
                        currentLocation + new Point(1, 1),
                    }
                    : new[]
                    {
                        currentLocation + new Point(-1, -1),
                        currentLocation + new Point(-1, 0),
                        currentLocation + new Point(-1, 1),
                        currentLocation + new Point(1, -1),
                        currentLocation + new Point(1, 0),
                        currentLocation + new Point(1, 1),
                    };

                if (cellsToCheck.All(c => grid[c.X, c.Y] == 0))
                {
                    var letter = grid[currentLocation.X, currentLocation.Y] & 0x7F;
                    letterSeeds[(char)letter].Add(currentLocation);
                }

                currentLocation += delta;
            }
        }

        private void UndoWordPlacement(Backtrack backtrack)
        {
            Console.WriteLine("Backtracking");
            
            foreach (var location in backtrack.GetLetterPlacedLocations())
            {
                grid[location.X, location.Y] = 0;
            }

            // WYLO: Much better. We still have some words that fail to place, but
            // only 1 out of about 500, which is quite a bit better.
            
            foreach (var removedSeed in backtrack.GetRemovedSeeds())
            {
                var letterAtSeed = grid[removedSeed.X, removedSeed.Y] & 0x7F;
                letterSeeds[(char)letterAtSeed].Add(removedSeed);
            }
            
            PushWord(backtrack.Word);
        }

        private string PopWord()
        {
            var word = unplacedWords.Last();
            unplacedWords.RemoveAt(unplacedWords.Count - 1);
            return word;
        }

        private void PushWord(string word)
        {
            // I just have this method for symmetry; calling PopWord and List<T>.Add
            // feels lazy.
            unplacedWords.Add(word);
        }

        private void PushWordAsLast(string word)
        {
            unplacedWords.Insert(0, word);
        }

        private static Point GetDelta(WordPlaceDirection direction) =>
            direction == WordPlaceDirection.Down
                ? new Point(0, 1)
                : new Point(1, 0);
    }

    internal sealed class Backtrack
    {
        public string Word { get; }
        private readonly Point[] letterPlacedLocations;
        private readonly Point[] removedSeeds;

        public Backtrack(string word, IEnumerable<Point> letterPlacedLocations, IEnumerable<Point> removedSeeds)
        {
            Word = word;
            this.removedSeeds = removedSeeds.ToArray();
            this.letterPlacedLocations = letterPlacedLocations.ToArray();
        }

        public IEnumerable<Point> GetLetterPlacedLocations() => letterPlacedLocations;
        public IEnumerable<Point> GetRemovedSeeds() => removedSeeds;
    }

    internal enum WordPlaceDirection
    {
        Down,
        Right
    }

    internal enum CanPlaceLetterResult
    {
        Yes,
        DesiredLetterAlreadyPresent,
        No
    }
}
