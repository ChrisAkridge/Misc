using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.JustForFun.FootballSimulator.Data.Models;

namespace Celarix.JustForFun.FootballSimulator
{
    internal static class ScheduleGenerator
    {
        public sealed class DivisionMatchup
        {
            public Conference ConferenceA { get; set; }
            public Division DivisionA { get; set; }
            public Conference ConferenceB { get; set; }
            public Division DivisionB { get; set; }
        }
        
        public static List<GameRecord> GeneratePreseasonAndRegularSeason(List<Team> teams,
            int calendarYear)
        {
            
        }

        private static List<DateTimeOffset> GetRegularSeasonWeekStartDatesForYear(int calendarYear)
        {
            // Week 1 starts on the second Thursday of September
            
        }
    }
}
