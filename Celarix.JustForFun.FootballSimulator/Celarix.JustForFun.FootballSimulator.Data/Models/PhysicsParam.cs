using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Data.Models
{
    public class PhysicsParam(string name, double value, string unit, string unitPlural,
        double? minValue = null, double? maxValue = null, double? precision = null)
    {
        [Key]
        public int PhysicsParamID { get; set; }
        public string Name { get; set; } = name;
        public double Value { get; set; } = value;

        // Note: these next five properties are only for reference in a UI with sliders and labels;
        // they intentionally do not constrain the Value property in any way.
        public string Unit { get; set; } = unit;
        public string UnitPlural { get; set; } = unitPlural;
        public double MinValue { get; set; } = minValue ?? 0;
        public double MaxValue { get; set; } = maxValue ?? Math.Max(Math.Abs(value * 10), 10);
        public double Precision { get; set; } = precision ?? Math.Max(Math.Abs(value / 10), 0.01);

        public PhysicsParam(string name, double value, string unit)
            : this(name, value, unit, unit + "s")
        {
        }
    }
}
