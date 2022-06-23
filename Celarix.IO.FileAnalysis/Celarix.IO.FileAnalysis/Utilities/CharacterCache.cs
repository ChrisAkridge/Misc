using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Celarix.IO.FileAnalysis.Utilities
{
    internal static class CharacterCache
    {
        private static Rgb24[][,] cache;

        public static void Build()
        {
            // cache = new Rgb24[7,10][95];
            cache = new Rgb24[95][,];
            var font = SystemFonts.CreateFont("Consolas", 14f);

            for (int i = 0; i < 95; i++)
            {
                char characterToCache = (char)(i + 0x20);
                using var characterImage = new Image<Rgb24>(7, 24, new Rgb24(0, 0, 0));
                characterImage.Mutate(ctx => ctx.DrawText(characterToCache.ToString(), font, Color.White, PointF.Empty));

                cache[i] = new Rgb24[7, 24];
                for (var y = 0; y < 24; y++)
                {
                    for (var x = 0; x < 7; x++)
                    {
                        cache[i][x, y] = characterImage[x, y];
                    }
                }
            }
        }

        public static void DrawString(Image<Rgb24> image, string text, Point location)
        {
            var currentLocation = location;
            foreach (var cachedCharacter in text.Select(c => cache[c - 0x20]))
            {
                if (currentLocation.X + 7 >= image.Width)
                {
                    break;
                }
                
                for (var y = 0; y < 24; y++)
                {
                    for (var x = 0; x < 7; x++)
                    {
                        image[x + currentLocation.X, y + currentLocation.Y] = cachedCharacter[x, y];
                    }
                }
                currentLocation = new Point(currentLocation.X + 7, currentLocation.Y);
            }
        }
    }
}
