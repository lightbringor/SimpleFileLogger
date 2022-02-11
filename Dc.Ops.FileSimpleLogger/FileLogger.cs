using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dc.Ops.SimpleFileLogger
{
    public class FileLogger : ILogger
    {
        private readonly FileLoggerProvider provider;
        private readonly string filePath;

        public FileLogger(FileLoggerProvider provider, string filePath)
        {
            this.provider = provider;
            this.filePath = filePath;

        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(string fullFilePath, LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var logRecord = string.Format("{0} [{1}] : {2} {3}\n",
            "[" + DateTime.Now.ToString("HH:mm:ss") + "]",
            logLevel.ToString(),
            formatter(state, exception),
            exception != null ? $"\n{exception}" : "");

            provider.AddToLogQueue(new LogMessage(fullFilePath, logRecord));
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var fullFilePath = $"{filePath}_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            Log(fullFilePath, logLevel, eventId, state, exception, formatter);

        }
    }
}
