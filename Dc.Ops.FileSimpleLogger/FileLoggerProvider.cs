using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dc.Ops.SimpleFileLogger
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly IOptions<LoggerOptions> options;
        private readonly string logFolder;
        private readonly BlockingCollection<LogMessage> logQueue = new BlockingCollection<LogMessage>();
        private readonly Task processQueueTask;


        public FileLoggerProvider(IOptions<LoggerOptions> options) // options get provided by DI
        {
            this.options = options;

            logFolder = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location) ?? "", options.Value.LogFolder);
            if (logFolder == null)
                logFolder = "./";

            processQueueTask = Task.Factory.StartNew(
                () => ProcessLogMessageQueue(),
                TaskCreationOptions.LongRunning);
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

            var filePath = Path.Combine(logFolder, fileName);

            // fileName might contain sub directories, therefore check the directory existance of the full path
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return new FileLogger(this, filePath);
        }

        internal void AddToLogQueue(LogMessage logMessage)
        {
            try
            {
                if (!logQueue.IsCompleted)
                    logQueue.Add(logMessage);
            }
            catch (Exception exc)
            {
                try { File.WriteAllText(Path.Combine(logFolder, "LoggingFailures.log"), $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {exc}"); }
                catch (Exception) { }// if loging the log error also fails, don't do anything
            }
        }

        private void ProcessLogMessageQueue()
        {
            foreach (var message in logQueue.GetConsumingEnumerable())
            {
                // if the are a lot of frequent log messages, opening and closing the file every time might be unperformant ==> should be reconsidered
                File.AppendAllText(message.FullFilePath, message.Content);
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
