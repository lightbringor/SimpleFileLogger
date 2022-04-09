using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public class LoggerOptions
    {
        public string LogFolder { get; set; } = "./";
        public Dictionary<string, string> FileNamesWithoutExtension { get; set; } = new Dictionary<string, string>();
        public IEnumerable<EventOptions>? EventOptionsCollection { get; set; }
    }

    
}
