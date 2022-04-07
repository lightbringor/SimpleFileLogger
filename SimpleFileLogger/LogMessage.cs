using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    internal class LogMessage
    {
        public LogMessage(string fullFilePath, string content)
        {
            FullFilePath = fullFilePath;
            Content = content;
        }

        public string FullFilePath { get; } = "";
        public string Content { get; } = "";
    }
}
