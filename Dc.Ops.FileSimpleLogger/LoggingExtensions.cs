using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dc.Ops.SimpleFileLogger
{
    internal static class LoggingExtensions
    {
        static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        public static string ToJson(this object obj)
        {
            return JsonSerializer.Serialize(obj, jsonOptions);
        }
    }
}
