// Word list assumptions:
// - All words have only ASCII characters
// - No NULs in any word

using Celarix.JustForFun.WordscapesGenerator;

const string wordListPath = @"C:\Users\celarix\Documents\english_wordlist.txt";
var comparer = new WordUniqueLetterCountComparer();
var words = File.ReadAllLines(wordListPath)
    .Where(w => w.Length >= 3)
    .OrderBy(w => w, comparer)
    .ToArray();

Console.WriteLine($"Loaded {words.Length} words from {wordListPath}");

for (var i = 0; i < words.Length; i++)
{
    words[i] = new string(words[i]
        .Select(c => char.IsAscii(c) && c >= ' '
            ? char.IsLetter(c)
                ? char.ToUpper(c)
                : c
            : ' ')
        .ToArray());
}

Console.WriteLine("Converted words to ASCII");
Console.WriteLine("Shuffled words");

var grid = new WordscapesGrid(words);
var placedWords = 0;

// TODO: also make a version starting with TITIN

while (!grid.PlacementComplete)
{
    Console.WriteLine($"Placing word #{placedWords}: {grid.NextWord}");
    grid.PlaceNextWord();
    placedWords += 1;
}