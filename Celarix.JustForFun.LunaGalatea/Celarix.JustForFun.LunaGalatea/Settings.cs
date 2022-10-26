using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Celarix.JustForFun.LunaGalatea.Logic.FootballSimulatorLite;
using Celarix.JustForFun.LunaGalatea.Logic.Yahtzee;

namespace Celarix.JustForFun.LunaGalatea
{
    public sealed class Settings
    {
        public int RandomValueUpdateTime { get; set; }
        public int StaticURLImagePresenterUpdateTime { get; set; }
        public FootballInfo FootballInfo { get; set; }
        public YahtzeeInfo YahtzeeInfo { get; set; }

        private Settings() { }
        
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
                    FootballInfo = GetDefaultFootballInfo(),
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

        private static FootballInfo GetDefaultFootballInfo()
        {
            var random = new Random();
            var locations = new[]
            {
                "Baltimore",
                "Buffalo",
                "Cincinnati",
                "Cleveland",
                "Denver",
                "Houston",
                "Indianapolis",
                "Jacksonville",
                "Kansas City",
                "Las Vegas",
                "Los Angeles",
                "Miami",
                "New England",
                "New York",
                "Pittsburgh",
                "Tennessee",
                "Arizona",
                "Atlanta",
                "Chicago",
                "Dallas",
                "Detroit",
                "Green Bay",
                "Los Angeles",
                "Minnesota",
                "New Orleans",
                "New York",
                "Philadelphia",
                "San Francisco",
                "Seattle",
                "Tampa Bay",
                "Washington"
            };

            var names = new[]
            {
                "Ravens",
                "Bills",
                "Bengals",
                "Browns",
                "Broncos",
                "Texans",
                "Colts",
                "Jaguars",
                "Chiefs",
                "Raiders",
                "Chargers",
                "Dolphins",
                "Patriots",
                "Jets",
                "Steelers",
                "Titans",
                "Cardinals",
                "Falcons",
                "Panthers",
                "Bears",
                "Cowboys",
                "Lions",
                "Packers",
                "Rams",
                "Vikings",
                "Saints",
                "Giants",
                "Eagles",
                "49ers",
                "Seahawks",
                "Buccaneers",
                "Commanders"
            };

            var abbreviations = new[]
            {
                "BAL", "BUF", "CIN", "CLE",
                "DEN", "HOU", "IND", "JAX",
                "KC",  "LV",  "LA",  "MIA",
                "NE",  "NYJ", "PIT", "TEN",
                "ARI", "ATL", "CAR", "CHI",
                "DAL", "DET", "GB",  "LA",
                "MIN", "NO",  "NYG", "PHI",
                "SF",  "SEA", "TB",  "WAS"
            };

            var teams = new FootballTeam[32];
            for (int i = 0; i < 32; i++)
            {
                teams[i] = new FootballTeam
                {
                    Location = locations[i],
                    Name = names[i],
                    Abbreviation = abbreviations[i],
                    TotalGames = 0,
                    Wins = 0,
                    Ties = 0,
                    PointsScored = 0,
                    PointsAllowed = 0,
                    KickoffStrength = random.Next(1, 1000),
                    KickReturnStrength = random.Next(1, 1000),
                    PuntingStrength = random.Next(1, 1000),
                    FreeKickStrength = random.Next(1, 1000),
                    FieldGoalStrength = random.Next(1, 1000),
                    RushingStrength = random.Next(1, 1000),
                    PassingStrength = random.Next(1, 1000),
                    RushDefenseStrength = random.Next(1, 1000),
                    PassDefenseStrength = random.Next(1, 1000),
                    BallCarryStrength = random.Next(1, 1000)
                };
            }

            return new FootballInfo
            {
                Year = DateTimeOffset.UtcNow.Year,
                Week = 1,
                GameNumber = 1,
                Teams = teams
            };
        }
    }
}
