using Celarix.IO.FileDistributions;
using Celarix.IO.FileDistributions.Distributions;

using var stream = File.OpenRead(@"G:\Documents\Packer_20220708012913.json");
using var binaryStream = new BinaryReader(stream);
var distribution = new ByteDistribution();
var sampleBits = new int[4];
try
{
    while (true)
    {
        var sample = binaryStream.ReadByte();
        distribution.AddSample(sample);
    }
}
catch (EndOfStreamException)
{
}

Console.WriteLine(distribution.GetDataText());