﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public static class SeedData
    {
        private static Random random = new Random();
        
        private static Team CreateTeam(string cityName, string teamName, string abbreviation, Conference conference,
            Division division, Stadium homeStadium) =>
            new Team
            {
                Abbreviation = abbreviation,
                CityName = cityName,
                TeamName = teamName,
                Conference = conference,
                Division = division,
                HomeStadium = homeStadium,
                RunningOffenseStrength = GetRandomStrength(),
                RunningDefenseStrength = GetRandomStrength(),
                PassingOffenseStrength = GetRandomStrength(),
                PassingDefenseStrength = GetRandomStrength(),
                OffensiveLineStrength = GetRandomStrength(),
                DefensiveLineStrength = GetRandomStrength(),
                KickingStrength = GetRandomStrength(),
                FieldGoalStrength = GetRandomStrength(),
                KickReturnStrength = GetRandomStrength(),
                KickDefenseStrength = GetRandomStrength(),
                ClockManagementStrength = GetRandomStrength(),
                Disposition = TeamDisposition.Conservative
            };

        private static double GetRandomStrength() => 200d + (random.NextDouble() * 800d);

        private static Stadium CreateStadium(string name, string city, double totalPrecipitationOverSeason, double averageWindSpeed, params double[] averageTemperatures) =>
            new Stadium
            {
                Name = name,
                City = city,
                AverageTemperatures = string.Join(",", averageTemperatures),
                TotalPrecipitationOverSeason = totalPrecipitationOverSeason,
                AverageWindSpeed = averageWindSpeed
            };

        public static List<Team> TeamSeedData()
        {
            var jetsGiantsStadium = CreateStadium("MetLife Stadium", "East Rutherford, NJ", 28, 7.1, 84, 76, 64,
                54, 43, 38, 41);
            
            return new List<Team>
            {
                CreateTeam("Cincinnati", "Bengals", "CIN", Conference.AFC, Division.North,
                    CreateStadium("Paycor Stadium", "Cincinnati, OH", 23.32, 9, 85.2, 78.9, 66.7,
                        53.8, 43.3, 39.6, 43.7)),
                CreateTeam("Baltimore", "Ravens", "BAL", Conference.AFC, Division.North,
                    CreateStadium("M&T Bank Stadium", "Baltimore, MD", 25.29, 8.7, 86.5, 79.7, 68.3,
                        57.3, 47.5, 43.2, 46.4)),
                CreateTeam("Pittsburgh", "Steelers", "PIT", Conference.AFC, Division.North,
                    CreateStadium("Acrisure Stadium", "Pittsburgh, PA", 20, 9, 81.7, 75.1, 63.1,
                        50.9, 40.6, 36.3, 39.6)),
                CreateTeam("Cleveland", "Browns", "CLE", Conference.AFC, Division.North,
                    CreateStadium("FirstEnergy Field", "Cleveland, OH", 22.93, 10.5, 82, 75.6, 63.7,
                        51.3, 40.4, 35.8, 38.5)),
                CreateTeam("Indianapolis", "Colts", "IND", Conference.AFC, Division.South,
                    CreateStadium("Lucas Oil Stadium", "Indianapolis, IN", 21.48, 9.6, 84.3, 78.2, 65.6,
                        51.8, 40.4, 36.1, 40.8)),
                CreateTeam("Tennessee", "Titans", "TEN", Conference.AFC, Division.South,
                    CreateStadium("Nissan Stadium", "Nashville, TN", 27.73, 8, 90.4, 84.4, 73.5,
                        61.4, 52.2, 49.1, 53.8)),
                CreateTeam("Houston", "Texans", "HOU", Conference.AFC, Division.South,
                    CreateStadium("NRG Stadium", "Houston, TX", 29.64, 7.6, 94.9, 90.4, 82.8,
                        72.6, 65.3, 63.8, 67.8)),
                CreateTeam("Jacksonville", "Jaguars", "JAX", Conference.AFC, Division.South,
                    CreateStadium("TIAA Bank Field", "Jacksonville, FL", 29.39, 7.8, 90.8, 87.2, 80.9,
                        73.2, 67.5, 65.5, 68.9)),
                CreateTeam("Buffalo", "Bills", "BUF", Conference.AFC, Division.East,
                    CreateStadium("Highmark Stadium", "Orchard Park, NY", 24.45, 11.8, 79, 72.3, 59.6,
                        47.8, 37.2, 32.1, 33.3)),
                CreateTeam("New England", "Patriots", "NE", Conference.AFC, Division.East,
                    CreateStadium("Gilette Stadium", "Foxborough, MA", 28.36, 14.53, 81, 73, 62,
                        52, 41, 36, 40)),
                CreateTeam("Miami", "Dolphins", "MIA", Conference.AFC, Division.East,
                    CreateStadium("Hard Rock Stadium", "Miami Gardens, FL", 37.4, 9.2, 90.7, 89, 85.9,
                        81.3, 78.2, 76.2, 78.2)),
                CreateTeam("New York", "Jets", "NYJ", Conference.AFC, Division.East, jetsGiantsStadium),
                CreateTeam("Denver", "Broncos", "DEN", Conference.AFC, Division.West,
                    CreateStadium("Empower Field at Mile High", "Denver, CO", 5.7, 8.7, 87.5, 79.6, 65.3,
                        52.9, 44, 44.6, 45.7)),
                CreateTeam("Oakland", "Raiders", "OAK", Conference.AFC, Division.West,
                    CreateStadium("Oakland Coliseum", "Oakland, CA", 41.5, 8.3, 72.8, 74.6, 72.7,
                        65.8, 59.7, 59.8, 62.4)),
                CreateTeam("San Diego", "Chargers", "SD", Conference.AFC, Division.West,
                    CreateStadium("San Diego Stadium", "San Diego, CA", 7.27, 7, 77.3, 77.2, 74.6,
                        70.7, 66, 66.4, 66.2)),
                CreateTeam("Kansas City", "Chiefs", "KC", Conference.AFC, Division.West,
                    CreateStadium("Arrowhead Stadium", "Kansas City, MO", 17.2, 10.6, 88.6, 80.4, 68.2,
                        54.5, 43.9, 39.9, 45.1)),
                CreateTeam("Detroit", "Lions", "DET", Conference.NFC, Division.North,
                    CreateStadium("Ford Field", "Detroit, MI", 18.14, 10.2, 81.4, 74.4, 62,
                        48.6, 37.2, 32.3, 35.2)),
                CreateTeam("Green Bay", "Packers", "GB", Conference.NFC, Division.North,
                    CreateStadium("Lambeau Field", "Green Bay, WI", 15.58, 12, 78.9, 71.7, 58,
                        43.5, 31.1, 25.5, 29)),
                CreateTeam("Chicago", "Bears", "CHI", Conference.NFC, Division.North,
                    CreateStadium("Soldier Field", "Chicago, IL", 20.77, 10.3, 83.1, 76.5, 63.7,
                        49.6, 37.7, 32.8, 36.8)),
                CreateTeam("Minnesota", "Vikings", "MIN", Conference.NFC, Division.North,
                    CreateStadium("U.S. Bank Stadium", "Minneapolis, MN", 14.48, 10.5, 80.7, 72.9, 58.1,
                        41.9, 28.8, 23.6, 28.5)),
                CreateTeam("Tampa Bay", "Buccaneers", "TB", Conference.NFC, Division.South,
                    CreateStadium("Raymond James Stadium", "Tampa, FL", 26.7, 8.3, 91.2, 90.2, 85.6,
                        78.9, 73.9, 71.3, 74)),
                CreateTeam("Atlanta", "Falcons", "ATL", Conference.NFC, Division.South,
                    CreateStadium("Mercedes-Benz Stadum", "Atlanta, GA", 29.09, 9.1, 89, 83.9, 74.4,
                        64.1, 56.2, 54, 58.2)),
                CreateTeam("Carolina", "Panthers", "CAR", Conference.NFC, Division.South,
                    CreateStadium("Bank of America Stadium", "Charlotte, NC", 62.6, 7.4, 88.6, 82.8, 73.3,
                        62.9, 54.9, 52.3, 56.6)),
                CreateTeam("New Orleans", "Saints", "NO", Conference.NFC, Division.South,
                    CreateStadium("Caesars Superdome", "New Orleans, NO", 33.72, 8.2, 91.3, 88.1, 80.6,
                        71.2, 64.8, 62.5, 66.4)),
                CreateTeam("Philadelphia", "Eagles", "PHI", Conference.NFC, Division.East,
                    CreateStadium("Lincoln Financial Field", "Philadelphia, PA", 24.93, 9.5, 85.8, 78.9, 67.2,
                        55.9, 46, 41.3, 44.3)),
                CreateTeam("Dallas", "Cowboys", "DAL", Conference.NFC, Division.East,
                    CreateStadium("AT&T Stadium", "Arlington, TX", 20.66, 15.79, 94.4, 86.6, 76.5,
                        65, 56.3, 54.7, 59.1)),
                CreateTeam("New York", "Giants", "NY", Conference.NFC, Division.East, jetsGiantsStadium),
                CreateTeam("Washington", "Presidents", "WAS", Conference.NFC, Division.East,
                    CreateStadium("FedExField", "Landover, MD", 25.18, 8.1, 87.8, 80.7, 69.4,
                        58.2, 48.8, 44.8, 48.3)),
                CreateTeam("San Francisco", "49ers", "SF", Conference.NFC, Division.West,
                    CreateStadium("Levi's Stadium", "Santa Clara, CA", 10.66, 7, 82, 81, 76,
                        67, 59, 58, 62)),
                CreateTeam("Seattle", "Seahawks", "SEA", Conference.NFC, Division.West,
                    CreateStadium("Lumen Field", "Seattle, WA", 28.06, 8.8, 77.6, 71.6, 60.5,
                        52.1, 47, 48, 50.3)),
                CreateTeam("Arizona", "Cardinals", "ARI", Conference.NFC, Division.West,
                    CreateStadium("State Farm Stadium", "Glendale, AZ", 5.11, 6.75, 105.1, 100.4, 89.2,
                        76.5, 66.2, 67.6, 70.8)),
                CreateTeam("St. Louis", "Rams", "STL", Conference.NFC, Division.West,
                    CreateStadium("The Dome at America's Center", "St. Louis, MO", 20.23, 9.6, 89.6, 88.3, 81.1,
                        69.2, 55.5, 44.5, 40.4)), // I still believe
                CreateTeam("Louisville", "Thunder", "LOU", Conference.AFC, Division.Extra,
                    CreateStadium("Cardinal Stadium", "Louisville, KY", 25.44, 9, 88.4, 82.2, 70.5,
                        57.6, 47.2, 43.6, 48.3)),
                CreateTeam("Vostok Station", "Penguins", "VOS", Conference.AFC, Division.Extra,
                    CreateStadium("Bellingshausen Stadium", "Vostok Station, AQ", 0.39, 11, -83, -78.9, -60.7,
                        -35, -16.8, -16.6, -37.7)),
                CreateTeam("Toledo", "Mud Hens", "TOL", Conference.AFC, Division.Extra,
                    CreateStadium("Glass Bowl", "Toledo, OH", 18.41, 10.4, 84.1, 77.7, 65,
                        51.1, 39.4, 34.7, 37.8)),
                CreateTeam("Portales", "Gladiators", "POR", Conference.AFC, Division.Extra,
                    CreateStadium("Roosevelt General Stadium", "Portales, NM", 9.19, 10.7, 89.7, 84.4, 74.9,
                        62.9, 53.4, 54.3, 59.3)),
                CreateTeam("Furnace Creek", "Reapers", "FUR", Conference.NFC, Division.Extra,
                    CreateStadium("The Crucible", "Furnace Creek, CA", 1.57, 5.95, 115.9, 107.7, 93.3,
                        77.4, 65.6, 67.2, 73.7)),
                CreateTeam("Dover", "Wasps", "DOV", Conference.NFC, Division.Extra,
                    CreateStadium("Alumni Stadium", "Dover, DE", 27.09, 9.8, 85.1, 79.2, 68.9,
                        58, 48.6, 44.4, 47.4)),
                CreateTeam("Grand Forks", "Hawks", "GRF", Conference.NFC, Division.Extra,
                    CreateStadium("Alerus Center", "Grand Forks, ND", 9.53, 10.95, 79.8, 70.4, 53.9,
                        35.7, 21.4, 15.8, 20.5)),
                CreateTeam("Wainwright", "Glaciers", "WAI", Conference.NFC, Division.Extra,
                    CreateStadium("Chukchi Stadium", "Wainwright, AK", 1.43, 11.95, 48.5, 38.9, 25.7,
                        11.8, -0.3, -3.9, -4.7)
                )
            };
        }
    }
}
