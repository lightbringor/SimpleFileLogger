using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public class FileLogger : ILogger
    {
        private readonly IFileLoggerProvider provider;
        private readonly string fileName;

        public FileLogger(IFileLoggerProvider provider, string fileName)
        {
            this.provider = provider;
            this.fileName = fileName;

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
            var nameExtension = "";
            var subFolder = "";
            if (provider.EventOptionsDict.ContainsKey(eventId.Id))
            {
                var eventOptions = provider.EventOptionsDict[eventId.Id];
                if (eventOptions.NameExtensionFromEventName)
                {
                    if (!string.IsNullOrEmpty(eventId.Name))
                        nameExtension = $"_{eventId.Name}";
                    else
                        nameExtension = $"_{eventId.Id.ToString()}";
                }
                else if (eventOptions.NameExtension != null)
                {
                    nameExtension = eventOptions.NameExtension;
                }

                if (eventOptions.SubFolderFromEventName)
                {
                    if (!string.IsNullOrEmpty(eventId.Name))
                        subFolder = eventId.Name;
                    else
                        subFolder = eventId.Id.ToString();
                }
                else if (eventOptions.SubFolder != null)
                {
                    subFolder = eventOptions.SubFolder;
                }
            }

            var filePath = Path.Combine(provider.LogFolder, subFolder, fileName);
            // fileName might contain sub directories, therefore check the directory existance of the full path
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);


            var fullFilePath = $"{filePath}{nameExtension}_{DateTime.Now.ToString("yyyy-MM-dd")}.log";

            Log(fullFilePath, logLevel, eventId, state, exception, formatter);
        }
    }
}
