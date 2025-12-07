using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class PhysicsParam
    {
        [Key]
        public int PhysicsParamID { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }

        // Note: these next five properties are only for reference in a UI with sliders and labels;
        // they intentionally do not constrain the Value property in any way.
        public string Unit { get; set; }
        public string UnitPlural { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double Precision { get; set; }

        public PhysicsParam(string name, double value, string unit, string unitPlural,
            double? minValue = null, double? maxValue = null, double? precision = null)
        {
            Name = name;
            Value = value;
            Unit = unit;
            UnitPlural = unitPlural;
            MinValue = minValue ?? 0;
            MaxValue = maxValue ?? Math.Max(Math.Abs(value * 10), 10);
            Precision = precision ?? Math.Max(Math.Abs(value / 10), 0.01);
        }
    }
}
