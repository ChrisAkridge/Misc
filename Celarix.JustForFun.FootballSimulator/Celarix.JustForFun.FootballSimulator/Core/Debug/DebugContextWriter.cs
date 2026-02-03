using System;
using System.Collections.Generic;
using System.Text;

namespace Celarix.JustForFun.FootballSimulator.Core.Debug
{
    internal sealed class DebugContextWriter
    {
        private bool enabled;
        private string folderPath;
        private StreamWriter? streamWriter;

        public DebugContextWriter(bool enabled, string folderPath)
        {
            this.enabled = enabled;
            this.folderPath = folderPath;
            if (enabled)
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public void StartNewGame(int gameID)
        {
            if (streamWriter != null)
            {
                streamWriter.Flush();
                streamWriter.Close();
                streamWriter.Dispose();
                streamWriter = null;
            }

            var now = DateTimeOffset.Now.ToString("yyyyMMdd_hhmmss");
            var filePath = Path.Combine(folderPath, $"Game_{gameID}_{now}.log");
            streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
        }
    }
}
