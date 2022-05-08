using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public class FileLoggerOptions
    {
        public string LogFolder { get; set; } = "./";
        public Dictionary<string, string> FileNamesWithoutExtension { get; set; } = new Dictionary<string, string>();
        public IEnumerable<EventOptions>? EventOptions { get; set; }
        public int? NumberOfDaysToKeepLogs { get; set;}
    }

    public class CleanupOtions {
        public bool Active { get; set; }
        
    }

    
}
