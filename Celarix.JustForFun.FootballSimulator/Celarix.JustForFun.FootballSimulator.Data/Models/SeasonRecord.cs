using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class SeasonRecord
    {
        public int SeasonID { get; set; }
        public int Year { get; set; }
        
        public List<GameRecord> GameRecords { get; set; }
    }
}
