using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public interface IFileLoggerProvider : ILoggerProvider
    {
        string LogFolder { get; }
        Dictionary<int, EventOptions> EventOptionsDict { get; }
        event EventHandler<MessageLoggedEventArgs> MessageLogged;

        void AddToLogQueue(LogMessage logMessage);
    }

    public class MessageLoggedEventArgs : EventArgs
    {
        public LogMessage LogMessage { get; set; }

        public MessageLoggedEventArgs(LogMessage logMessage)
        {
            LogMessage = logMessage;
        }
    }

    public class FileLoggerProvider : IFileLoggerProvider, ILoggerProvider
    {
        public Dictionary<int, EventOptions> EventOptionsDict { get; } = new Dictionary<int, EventOptions>();
        public string LogFolder { get; }

        /// <summary>
        /// This event is mainly to be used for testing in the example project and may not serve any purpose
        /// in a real applicaction
        /// </summary>
        public event EventHandler<MessageLoggedEventArgs>? MessageLogged;

        private readonly IOptions<FileLoggerOptions> options;
        private readonly BlockingCollection<LogMessage> logQueue = new BlockingCollection<LogMessage>();
        private readonly Task processQueueTask;
        private ILogger? cleanupLogger;
        private System.Timers.Timer? cleanLogsTimer;

        public FileLoggerProvider(IOptions<FileLoggerOptions> options) // options get provided by DI
        {
            this.options = options;

            if (options.Value.EventOptions != null)
            {
                foreach (var eventOptions in options.Value.EventOptions)
                {
                    EventOptionsDict[eventOptions.Id] = eventOptions;
                }
            }

            LogFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location) ?? "", options.Value.LogFolder);
            if (LogFolder == null)
                LogFolder = "./";

            if (options.Value.NumberOfDaysToKeepLogs > 0)
            {
                cleanupLogger = CreateLogger("CleanupLogFiles");
                cleanLogsTimer = new System.Timers.Timer(TimeSpan.FromSeconds(10).TotalMilliseconds);
                cleanLogsTimer.Elapsed += (s, e) => CleanOldLogs();
                cleanLogsTimer.AutoReset = true;
                cleanLogsTimer.Start();
            }

            processQueueTask = Task.Factory.StartNew(() => ProcessLogMessageQueue(), TaskCreationOptions.LongRunning);
        }

        private void CleanOldLogs()
        {
            try
            {
                cleanupLogger!.LogDebug("Checking existing log files, deleting everything older than {x} days", options.Value.NumberOfDaysToKeepLogs);
                var logFiles = Directory.GetFiles(LogFolder, "*.log", SearchOption.AllDirectories);
                foreach (var logFile in logFiles)
                {
                    var lastWriteTime = File.GetLastWriteTime(logFile);
                    var refDate = DateTime.Now.Date.Subtract(TimeSpan.FromDays((double)options.Value.NumberOfDaysToKeepLogs!));
                    if (lastWriteTime < refDate)
                    {
                        try
                        {
                            File.Delete(logFile);
                        }
                        catch (System.Exception exc)
                        {
                            cleanupLogger!.LogWarning(exc, "Deleting log file '{file}' failed!", logFile);
                        }
                    }
                }

            }
            catch (System.Exception exc)
            {
                cleanupLogger!.LogError(exc, "CleanOldLogs failed!");
                // var path = Path.Combine(LogFolder, "CleanOldLogsFailures.log");
                // var msg = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {exc}";
                // AddToLogQueue(new LogMessage(path, msg));
            }
        }

        public ILogger CreateLogger(string categoryName)
        {
            var fileName = options.Value.FileNamesWithoutExtension
                .Where(kvp => categoryName.StartsWith(kvp.Key) || kvp.Key == "*")
                .OrderByDescending(kvp => kvp.Key.Length)
                .Select(kvp => kvp.Value)
                .FirstOrDefault();
            if (fileName == null)
                fileName = "Default";

            return new FileLogger(this, fileName);
        }

        public void AddToLogQueue(LogMessage logMessage)
        {
            try
            {
                if (!logQueue.IsCompleted)
                    logQueue.Add(logMessage);
            }
            catch (Exception exc)
            {
                try { File.WriteAllText(Path.Combine(LogFolder, "LoggingFailures.log"), $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {exc}"); }
                catch (Exception) { }// if loging the log error also fails, don't do anything
            }
        }

        /// <summary>
        /// Runs as a separate Task. GetConsumingEnumerable() will remove items 
        /// from logQueue and block until new messageL arrive.
        /// </summary>
        private void ProcessLogMessageQueue()
        {
            foreach (var message in logQueue.GetConsumingEnumerable())
            {
                // if there are a lot of frequent log messages, opening and closing the file every time might be unperformant ==> should be reconsidered
                File.AppendAllText(message.FullFilePath, message.Content);
                MessageLogged?.Invoke(this, new MessageLoggedEventArgs(message));
            }
        }

        public void Dispose()
        {
            logQueue.CompleteAdding();
            try
            {
                processQueueTask.Wait(1000);
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }

        }
    }
}
