using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;

namespace Celarix.JustForFun.LunaGalatea
{
    public sealed class Settings
    {
        public int RandomValueUpdateTime { get; set; }
        public int StaticURLImagePresenterUpdateTime { get; set; }
        public int YahtzeePlayerUpdateTime { get; set; }
        public YahtzeeInfo YahtzeeInfo { get; set; }
        
        public static Settings LoadOrCreate()
        {
            var currentFolder = Directory.GetCurrentDirectory();
            var settingsPath = Path.Combine(currentFolder, "settings.json");
            if (!Directory.GetFiles(currentFolder, "settings.json", SearchOption.TopDirectoryOnly).Any())
            {
                var newSettings = new Settings
                {
                    RandomValueUpdateTime = 30,
                    StaticURLImagePresenterUpdateTime = 30,
                    YahtzeePlayerUpdateTime = 30,
                    YahtzeeInfo = new YahtzeeInfo()
                };
                var newSettingsJson = JsonSerializer.Serialize(newSettings);
                File.WriteAllText(settingsPath, newSettingsJson);
                return newSettings;
            }
            
            var settingsJson = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<Settings>(settingsJson) ?? throw new ArgumentNullException();
        }

        public void Save() =>
            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "settings.json"),
                JsonSerializer.Serialize(this));
    }
}
