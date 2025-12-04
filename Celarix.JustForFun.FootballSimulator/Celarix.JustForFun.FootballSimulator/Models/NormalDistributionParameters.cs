using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.JustForFun.FootballSimulator.Models
{
    [Obsolete]
    public sealed class NormalDistributionParameters
    {
        public double MeanAtZero { get; set; }
        public double StandardDeviationAtZero { get; set; }
        public double MeanReductionPerUnitValue { get; set; }
        public double StandardDeviationIncreasePerUnitValue { get; set; }

        public NormalDistributionParameters(double meanAtZero,
            double standardDeviationAtZero,
            double meanReductionPerUnitValue,
            double standardDeviationIncreasePerUnitValue)
        {
            MeanAtZero = meanAtZero;
            StandardDeviationAtZero = standardDeviationAtZero;
            MeanReductionPerUnitValue = meanReductionPerUnitValue;
            StandardDeviationIncreasePerUnitValue = standardDeviationIncreasePerUnitValue;
        }
    }
}
