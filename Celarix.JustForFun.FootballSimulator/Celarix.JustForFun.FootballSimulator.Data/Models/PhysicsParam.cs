using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class PhysicsParam
    {
        public int PhysicsParamID { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public string UnitPlural { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double Precision { get; set; }
    }
}
