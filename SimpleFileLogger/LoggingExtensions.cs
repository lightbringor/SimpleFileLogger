using Microsoft.Extensions.Configuration;
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
            if (obj != null && (logger == null || logger.IsEnabled(level)))
                return JsonSerializer.Serialize(obj, jsonOptions);
            return string.Empty;
        }

        public static IServiceCollection AddSimpleFileLogging(this IServiceCollection services, IConfiguration configuration, string configSection = "Logging:FileLoggerOptions")
        {
            services.AddLogging(logBuilder =>
            {
                logBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
                logBuilder.Services.Configure<FileLoggerOptions>(options => configuration.GetSection(configSection).Bind(options));
            });
            return services;
        }
    
        public static void LogCritical(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogCritical(fileName, 0, null, message, args);
        }

        public static void LogCritical(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogCritical(fileName, 0, exception, message, args);
        }

        public static void LogCritical(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Critical, eventId, exception, message, args);
        }


        public static void LogError(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogError(fileName, 0, null, message, args);
        }

        public static void LogError(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogError(fileName, 0, exception, message, args);
        }

        public static void LogError(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Error, eventId, exception, message, args);
        }
    

        public static void LogWarning(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogWarning(fileName, 0, null, message, args);
        }

        public static void LogWarning(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogWarning(fileName, 0, exception, message, args);
        }

        public static void LogWarning(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Warning, eventId, exception, message, args);
        }


        public static void LogInformation(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogInformation(fileName, 0, null, message, args);
        }

        public static void LogInformation(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogInformation(fileName, 0, exception, message, args);
        }

        public static void LogInformation(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Information, eventId, exception, message, args);
        }


        public static void LogDebug(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogDebug(fileName, 0, null, message, args);
        }

        public static void LogDebug(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogDebug(fileName, 0, exception, message, args);
        }

        public static void LogDebug(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Debug, eventId, exception, message, args);
        }


        public static void LogTrace(this ILogger logger, string fileName, string? message, params object?[] args)
        {           
            logger.LogDebug(fileName, 0, null, message, args);
        }

        public static void LogTrace(this ILogger logger, string fileName, Exception? exception, string? message, params object?[] args)
        {           
            logger.LogDebug(fileName, 0, exception, message, args);
        }

        public static void LogTrace(this ILogger logger, string fileName, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            logger.Log(fileName, LogLevel.Trace, eventId, exception, message, args);
        }

        public static void Log(this ILogger logger, string fileName, LogLevel logLevel, EventId eventId, Exception? exception, string? message, params object?[] args)
        {           
            (logger as IFileLogger)?.LogToFile(fileName, logLevel, eventId, exception, message, args);
        }
    }
}
