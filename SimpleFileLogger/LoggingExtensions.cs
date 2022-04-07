using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SimpleFileLogger
{
    public static class LoggingExtensions
    {
        static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        public static string ToJson(this object obj, ILogger? logger = null, LogLevel level = LogLevel.None)
        {
            if (obj!= null && (logger == null || logger.IsEnabled(level)))
                return JsonSerializer.Serialize(obj, jsonOptions);
            return string.Empty;
        }
    }
}
