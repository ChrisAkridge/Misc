using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celarix.Imaging.Progress;
using NLog;

namespace Celarix.IO.FileAnalysis.Analysis
{
    public sealed class StringProgressToLogger : IProgress<string>
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Reports a progress update.</summary>
        /// <param name="value">The value of the updated progress.</param>
        public void Report(string value) { logger.Trace(value); }
    }
}
