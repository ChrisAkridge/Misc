using Celarix.JustForFun.FootballSimulator.Data.Models;
using Celarix.JustForFun.FootballSimulator.Output.Models;
using Celarix.JustForFun.FootballSimulator.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Output
{
    public sealed class BasicConsoleListener : IGameEventListener
    {
        // Display properties
        private string awayTeamAbbreviation = "";
        private string homeTeamAbbreviation = "";
        private int awayScore = 0;
        private int homeScore = 0;
        private GameTeam teamWithPossession = GameTeam.Away;
        private string periodNumberDisplay = "";
        private string secondsRemainingInPeriodDisplay = "";
        private int awayTimeoutsRemaining = 0;
        private int homeTimeoutsRemaining = 0;
        private string nextPlay = "";
        private string distanceToGo = "";
        private string lineOfScrimmage = "";
        private string lastPlayDescription = "";

        public void Handle(GameEvent gameEvent)
        {
            var systemContext = gameEvent.SystemContext;
            var gameContext = gameEvent.GameContext;
            var playContext = gameEvent.PlayContext;

            if (gameContext == null && playContext == null)
            {
                Console.Clear();
                Console.WriteLine($"System state machine to go to {systemContext.NextState}.");
            }
            else if (playContext == null)
            {
                Console.Clear();
                Console.WriteLine($"Game state machine to go to {gameContext!.NextState}.");
            }

                var systemEnvironment = systemContext!.Environment;
            var gameEnvironment = gameContext!.Environment;
            var playEnvironment = playContext!.Environment;

            var currentGameRecord = gameEnvironment!.CurrentGameRecord!;
            var awayTeam = currentGameRecord.AwayTeam;
            var homeTeam = currentGameRecord.HomeTeam;

            awayTeamAbbreviation = awayTeam.Abbreviation;
            homeTeamAbbreviation = homeTeam.Abbreviation;
            awayScore = playContext.AwayScore;
            homeScore = playContext.HomeScore;
            teamWithPossession = playContext.TeamWithPossession;
            periodNumberDisplay = playContext.PeriodNumber.ToPeriodDisplayString();
            secondsRemainingInPeriodDisplay = playContext.SecondsLeftInPeriod.ToMinuteSecondString();
            awayTimeoutsRemaining = playContext.AwayTimeoutsRemaining;
            homeTimeoutsRemaining = playContext.HomeTimeoutsRemaining;
            lastPlayDescription = playContext.LastPlayDescriptionTemplate;
            distanceToGo = (playContext.NextPlay
                is NextPlayKind.FirstDown
                or NextPlayKind.SecondDown
                or NextPlayKind.ThirdDown
                or NextPlayKind.FourthDown)
                ? playContext.DistanceForFirstDown().ToString()
                : "";
            nextPlay = playContext.NextPlay switch
            {
                NextPlayKind.FirstDown => "1st & ",
                NextPlayKind.SecondDown => "2nd & ",
                NextPlayKind.ThirdDown => "3rd & ",
                NextPlayKind.FourthDown => "4th & ",
                NextPlayKind.Kickoff => "Kickoff",
                NextPlayKind.ConversionAttempt => "Conv",
                NextPlayKind.FreeKick => "Free Kick",
                _ => $"Unknown {playContext.NextPlay}"
            } + distanceToGo;
            
            lineOfScrimmage = playContext.InternalYardToDisplayTeamYardString(playContext.LineOfScrimmage, playEnvironment!.DecisionParameters);

            Render();
        }

        private void Render()
        {
            Console.Clear();

            var awayPossessionIndicator = teamWithPossession == GameTeam.Away ? "<" : " ";
            var homePossessionIndicator = teamWithPossession == GameTeam.Home ? ">" : " ";
            var scorePortion = $"{awayTeamAbbreviation} {awayPossessionIndicator} {awayScore,3} - {homeScore,3} {homePossessionIndicator} {homeTeamAbbreviation}";
            var clockPortion = $"{periodNumberDisplay} {secondsRemainingInPeriodDisplay}";
            var playAndDistancePortion = $"{nextPlay}, ball on {lineOfScrimmage}";
            var line1 = $"{scorePortion} | {clockPortion} | {playAndDistancePortion}";

            var awayTimeoutsPortion = new string('T', awayTimeoutsRemaining).PadRight(3, '-');
            var homeTimeoutsPortion = new string('T', homeTimeoutsRemaining).PadRight(3, '-');
            var line2 = $"{awayTimeoutsPortion}                      {homeTimeoutsPortion}";

            // Wrap last play description if too long
            var maxLineWidth = Console.WindowWidth;
            var lastPlayLines = new List<string>();
            if (lastPlayDescription.Length <= maxLineWidth)
            {
                lastPlayLines.Add(lastPlayDescription);
            }
            else
            {
                var words = lastPlayDescription.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var currentLine = new StringBuilder();
                foreach (var word in words)
                {
                    if (currentLine.Length + word.Length + 1 <= maxLineWidth)
                    {
                        if (currentLine.Length > 0)
                        {
                            currentLine.Append(' ');
                        }
                        currentLine.Append(word);
                    }
                    else
                    {
                        lastPlayLines.Add(currentLine.ToString());
                        currentLine.Clear();
                        currentLine.Append(word);
                    }
                }
                if (currentLine.Length > 0)
                {
                    lastPlayLines.Add(currentLine.ToString());
                }
            }

            Console.WriteLine(line1);
            Console.WriteLine(line2);
            foreach (var line in lastPlayLines)
            {
                Console.WriteLine(line);
            }
        }
    }
}
