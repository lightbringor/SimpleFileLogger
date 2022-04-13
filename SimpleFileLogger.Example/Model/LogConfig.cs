using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFileLogger.Example.Model
{
    public class LogConfig
    {
        public Dictionary<string, string>? LogLevel { get; set; }
        public FileLoggerOptions? FileLoggerOptions { get; set; }
    }
}