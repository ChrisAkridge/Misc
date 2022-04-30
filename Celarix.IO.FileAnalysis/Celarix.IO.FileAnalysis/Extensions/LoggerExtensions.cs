using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NLog;

namespace Celarix.IO.FileAnalysis.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogException(this Logger logger, Exception ex)
        {
            var typeName = ex.GetType().Name;
            var message = ex.Message;
            var stackTrace = ex.StackTrace;
            var innerExceptionTypeName = ex.InnerException?.GetType().Name;
            var innerExceptionMessage = ex.InnerException?.Message;
            var innerExceptionStackTrace = ex.InnerException?.StackTrace;
            var innerExceptionText = "";

            if (ex.InnerException != null)
            {
                innerExceptionText =
                    $"(Inner Exception: {innerExceptionTypeName}: {innerExceptionMessage}{Environment.NewLine}{innerExceptionStackTrace})";
            }
            
            logger.Info($"{typeName} thrown: {message} {innerExceptionText}{Environment.NewLine}{stackTrace}");
        }
    }
}
