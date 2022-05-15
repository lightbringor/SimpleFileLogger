using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public interface IFileLogger : ILogger
    {
        public void LogToFile(string fileName, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args);
    }
    public class FileLogger : ILogger, IFileLogger
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

        public void LogToFile(string fileName, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {
            var filePath = Path.Combine(provider.LogFolder, fileName);
            // fileName might contain sub directories, therefore check the directory existance of the full path
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);


            var fullFilePath = $"{filePath}_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
            string text = message != null ? string.Format(message, args) : "";
            var logRecord = BuildLogRecord(logLevel, text, exception, eventId);
            
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
            var logRecord = BuildLogRecord(logLevel, formatter(state, exception), exception, eventId);
            provider.AddToLogQueue(new LogMessage(fullFilePath, logRecord));
        }

        private string BuildLogRecord(LogLevel logLevel, string text, Exception? exception, EventId eventId)
        {
            var excText = exception != null ? $"\n{exception}" : "";
            var eventText = eventId.Id > 0 ? $"[{eventId.Name}(ID={eventId.Id})] " : "";
            var logRecord = $"[{DateTime.Now.ToString("HH:mm:ss")}] [{logLevel.ToString()}] {eventText}: {text} {excText}\n";

            return logRecord;
        }

    }
}
