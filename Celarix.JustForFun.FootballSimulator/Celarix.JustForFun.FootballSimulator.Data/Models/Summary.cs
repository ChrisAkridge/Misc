using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class Summary
    {
        [Key]
        public int SummaryID { get; set; }
        public int? GameRecordID { get; set; }
        public int? SeasonRecordID { get; set; }
        public string SummaryText { get; set; }
    }
}
