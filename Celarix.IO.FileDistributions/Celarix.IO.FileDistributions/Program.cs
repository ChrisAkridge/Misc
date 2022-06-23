using Celarix.IO.FileDistributions;

using var stream = File.OpenRead(@"C:\Users\celarix\Documents\circle_wave.wav");
var distribution = new UnderlyingDistribution(4);
int sample;
while ((sample = stream.ReadByte()) != -1)
{
    distribution.AddSample8OrFewer((byte)(sample >> 4));
    distribution.AddSample8OrFewer((byte)(sample & 0x0F));
}

Console.WriteLine(distribution.GetDataText());