using Celarix.JustForFun.FootballSimulator.Core.Debugging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Celarix.JustForFun.FootballSimulator.Core.Debugging
{
    internal sealed class DebugContextWriter : IDebugContextWriter
    {
        private readonly bool enabled;
        private readonly string folderPath;
        private StreamWriter? streamWriter;
        private JsonSerializerOptions jsonSerializerOptions;

        public DebugContextWriter(bool enabled, string folderPath)
        {
            this.enabled = enabled;
            this.folderPath = folderPath;
            jsonSerializerOptions = new JsonSerializerOptions();
            if (enabled)
            {
                Directory.CreateDirectory(folderPath);
            }

            // Start with a system log
            ExitGame();
        }

        public void EnterGame(int gameID)
        {
            if (!enabled)
            {
                return;
            }

            if (streamWriter != null)
            {
                streamWriter.Flush();
                streamWriter.Close();
                streamWriter.Dispose();
                streamWriter = null;
            }

            var now = DateTimeOffset.Now.ToString("yyyyMMdd_hhmmss");
            var filePath = Path.Combine(folderPath, $"{now}_Game_{gameID}.log");
            streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
        }

        public void ExitGame()
        {
            if (!enabled)
            {
                return;
            }

            if (streamWriter != null)
            {
                streamWriter.Flush();
                streamWriter.Close();
                streamWriter.Dispose();
                streamWriter = null;
            }

            var now = DateTimeOffset.Now.ToString("yyyyMMdd_hhmmss");
            var filePath = Path.Combine(folderPath, $"{now}_System.log");
            streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
        }

        public void WriteContext<T, TTagListable>(T context, TTagListable tagListable) where TTagListable : ITagListable
        {
            if (!enabled)
            {
                return;
            }
            var json = JsonSerializer.Serialize(new
            {
                Type = typeof(T).Name,
                Timestamp = DateTimeOffset.Now,
                Context = context,
                Tags = tagListable.GetTags().ToArray()
            }, jsonSerializerOptions);
            streamWriter!.WriteLine(json);
            streamWriter.Flush();
        }
    }
}
