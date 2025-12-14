using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Random;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator
{
    internal sealed class PlayerManager
    {
        private string[] firstNames;
        private string[] lastNames;

        public PlayerManager(IEnumerable<string> firstNames, IEnumerable<string> lastNames)
        {
            this.firstNames = [.. firstNames];
            this.lastNames = [.. lastNames];
        }

        public Player CreateNewPlayer(IRandom random, bool undraftedFreeAgent)
        {
            // Choose a name
            var firstName = random.Choice(firstNames);
            var lastName = random.Choice(lastNames);

            // Choose a date of between 18 and 30 years ago
            var ageInYears = random.Next(18, 31);
            var yearOfBirth = DateTimeOffset.Now.Year - ageInYears;
            var monthOfBirth = random.Next(1, 13);
            var dayOfBirth = random.Next(1, DateTime.DaysInMonth(yearOfBirth, monthOfBirth) + 1);
            var dateOfBirth = new DateTimeOffset(yearOfBirth, monthOfBirth, dayOfBirth, 0, 0, 0, TimeSpan.Zero);

            Log.Information("Created new player: {FirstName} {LastName}, born {DateOfBirth}",
                firstName,
                lastName,
                dateOfBirth.ToString("yyyy-MM-dd"));
            return new Player
            {
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                Retired = false,
                UndraftedFreeAgent = undraftedFreeAgent,
                RosterPositions = []
            };
        }

        public static void AssignPlayerToTeam(Player player,
            int teamID,
            BasicPlayerPosition position,
            IRandom random)
        {
            Log.Information("Assigning player {FirstName} {LastName} to team {TeamID} as {Position}",
                player.FirstName,
                player.LastName,
                teamID,
                position.ToString());
            player.RosterPositions.Add(new PlayerRosterPosition
            {
                TeamID = teamID,
                CurrentPlayer = true,
                JerseyNumber = random.Next(0, 100),
                Position = position
            });
        }

        public static IReadOnlyList<BasicPlayerPosition> GetStandardRoster()
        {
            return ((IEnumerable<BasicPlayerPosition>[])[Enumerable.Repeat(BasicPlayerPosition.Offense, 10),
                Enumerable.Repeat(BasicPlayerPosition.Quarterback, 1),
                Enumerable.Repeat(BasicPlayerPosition.Kicker, 1),
                Enumerable.Repeat(BasicPlayerPosition.Defense, 11)])
                .SelectMany(p => p)
                .ToArray();
        }
    }
}
