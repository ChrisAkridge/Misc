using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class SeasonRecord
    {
        [Key]
        public int SeasonRecordID { get; set; }
        public int Year { get; set; }
        public bool SeasonComplete { get; set; }

        public List<GameRecord>? GameRecords { get; set; }
    }
}
