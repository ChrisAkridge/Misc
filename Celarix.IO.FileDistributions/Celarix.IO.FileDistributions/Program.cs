using Celarix.IO.FileDistributions;
using Celarix.IO.FileDistributions.Distributions;

using var stream = File.OpenRead(@"C:\Users\celarix\Documents\circle_wave.wav");
using var binaryStream = new BinaryReader(stream);
var distribution = new CharDistribution();
var sampleBits = new int[4];
try
{
    while (true)
    {
        var sample = binaryStream.ReadInt16();
        distribution.AddSample((char)sample);
    }
}
catch (EndOfStreamException)
{
}

Console.WriteLine(distribution.GetDataText());