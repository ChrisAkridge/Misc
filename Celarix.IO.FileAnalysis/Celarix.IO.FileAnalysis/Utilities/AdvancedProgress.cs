using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celarix.IO.FileAnalysis.Utilities
{
    internal sealed class AdvancedProgress
    {
        public long CurrentAmount { get; set; }
        public long TotalAmount { get; set; }
        public long RemainingAmount => TotalAmount - CurrentAmount;
        public double Percentage => ((double)CurrentAmount / TotalAmount) * 100d;
        public DateTimeOffset ProgressStartedOn { get; set; }

        public double AmountPerSecond
        {
            get
            {
                var elapsed = DateTimeOffset.Now - ProgressStartedOn;
                return CurrentAmount / elapsed.TotalSeconds;
            }
        }

        public TimeSpan EstimatedRemainingTime => TimeSpan.FromSeconds(RemainingAmount / AmountPerSecond);
        public DateTimeOffset EstimatedCompletionTime => DateTimeOffset.Now + EstimatedRemainingTime;

        public AdvancedProgress(long totalAmount, DateTimeOffset? progressStartedOn)
        {
            TotalAmount = totalAmount;
            ProgressStartedOn = progressStartedOn ?? DateTimeOffset.Now;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() => $"{CurrentAmount}/{TotalAmount} ({Percentage:F2}%) | {AmountPerSecond:F2}/sec | est. {EstimatedCompletionTime:yyyy-MM-dd hh:mm:ss tt}";
    }
}
