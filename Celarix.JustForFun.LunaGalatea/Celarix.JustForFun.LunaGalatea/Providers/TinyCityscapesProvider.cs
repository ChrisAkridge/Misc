using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.LunaGalatea.Providers
{
    public sealed class TinyCityscapesProvider : IProvider<string>
    {
        private const double SkyCharacterProbability = 1d / 6d;

        public bool UseMonospaceFont => true;

        private readonly string[] skyCharacters = new[]
        {
            "🌘",
            "💧",
            "💫",
            "🌞",
            "🌧",
            "☄",
            "🌨",
            "🌦",
            "🌥",
            "🌪",
            "🌚",
            "🌩",
            "🌑",
            "🌕",
            "🌗",
            "🌟",
            "⛅",
            "✨",
            "⚡",
            "🌖",
            "🌤",
            "⛈",
            "☁",
        };

        private readonly string[] cityCharacters = new[]
        {
            "🎪",
            "🏛",
            "🏩",
            "🏥",
            "🕍",
            "🏠",
            "🏤",
            "⛪",
            "🏚",
            "🏨",
            "🏬",
            "🏢",
            "🕌",
            "🏪",
            "💒",
            "🏣",
            "🏫",
            "🏡",
            "🏦",
        };

        private readonly string[,] sky = new string[20, 2];
        private readonly string[,] city = new string[8, 2];
        private readonly Random random = new Random();
        private readonly StringBuilder builder = new StringBuilder();
        
        public string GetDisplayObject()
        {
            for (var i = 0; i < 20; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    sky[i, j] = random.NextDouble() < SkyCharacterProbability
                        ? skyCharacters[random.Next(0, skyCharacters.Length)]
                        : " ";
                }
            }

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    city[i, j] = cityCharacters[random.Next(0, cityCharacters.Length)];
                }
            }

            builder.Clear();
            for (var j = 0; j < 2; j++)
            {
                for (var i = 0; i < 20; i++) { builder.Append(sky[i, j]); }
                builder.Append(Environment.NewLine);
            }

            for (var j = 0; j < 2; j++)
            {
                for (var i = 0; i < 8; i++) { builder.Append(city[i, j]); }
                builder.Append(Environment.NewLine);
            }
            
            return builder.ToString();
        }
    }
}
