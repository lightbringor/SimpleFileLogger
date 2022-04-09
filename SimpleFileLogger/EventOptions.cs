using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public class EventOptions {
        public int Id { get; set; }
        public string? SubFolder { get; set; }
        public string? NameExtension { get; set; }
        public bool SubFolderFromEventName { get; set; }
        public bool NameExtensionFromEventName { get; set; }
    }
}