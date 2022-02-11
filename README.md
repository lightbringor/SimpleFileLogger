# Simple File Logger

Lightweight library for writing log information to files using the .NET `ILogger<T>` interface. For more information regarding the usage of `ILogger<T>` see the [Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0)

## Setup

**The minimum required framework version is `.NET 6`**

The library is available as a NuGet package from source `O:\Projects\Software Development\NuGet`

After including the package in the project, it can be added to the service collection in the startup code:

```csharp
using Dc.Ops.SimpleFileLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(logBuilder =>
{
    logBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
    logBuilder.Services.Configure<LoggerOptions>(options => builder.Configuration.GetSection("Logging:FileLoggerOptions").Bind(options));
});
```
Configuration is provided via `appsettings.json`

```json
    "Logging": { 
        "LogLevel": {
            "Default": "Debug",
            "Microsoft.AspNetCore": "Warning"
        },
        "FileLoggerOptions": {
            "LogFolder": "./Logs",
            "FileNamesWithoutExtension": {
                "Microsoft": "Framework/Microsoft",
                "Microsoft.EntityFrameworkCore": "Framework/EFCore",
                "Dc.Ops.Authentication": "App/General",
                "*": "Default"
            }
        }
    }
```
In the `LogLevel` section, the minimum level that leads to writing a log message to a file can be specified for namespaces or classes using the full qualified name. `Default` applies to all classes in namespaces not mentioned in the configuration. This is the .NET logging standard and nothing specific of *SimpleFileLogger*.

In the section below named `FileLoggerOptions` the options for file logging are provided. You can specify a root folder with the `LogFolder` value. If it is not set, the directory of the application assembly is used as root. Then a file name **without extension** can be specified for each namespace or class that shall be logged in its own file. The path is relative to `LogFolder`. The entry `*` is mandatory and specifies the default log file used for any content that falls not in one of the namespace defined above.

The file names provided here are extended by `_yyyy-MM-dd.log`. So a new log file is created every day

<mark>**Note: Currently old log files are not removed automatically which can result in high disk space usage when detailed log levels are enabled.**</mark>

`LogLevel`s can be change at any time without restarting the application. Changes in file names only apply after a restart.

That's all regarding the configuration. Now you only need to add a `ILogger<T>` to the constructor of classes you want to log information from and call the corresponding methods. Log information from existing framework classes will also be written automatically.

## Examples

```csharp
    // Constructor
    public MyClass(/*...*/ ILogger<MyClass> logger)
    {
        this.logger = logger;
        // ...
    }

    private void MyMethod()
    {
        try
        {
            object obj;
            // Do anything awesome
            logger.LogInformation("Did anything awesome to {obj}", obj)
        }
        catch(Exception exc)
        {
            logger.LogCritical(exc, "Exception!");
        }
    }
```

### Logging of complex objects

The library provides an extension method `ToJson()` for `object` that serializes the object to `JSON`.

```csharp
logger.LogError(exc, "Operation failed! Resource was:\n{resource}", newResource.ToJson());
```

<mark>**Note that Microsoft recommends to not us `$"{var} text"` interpolation but to use placeholders and parameters that are passed to the log method.**</mark>

## More

**SimpleFileLogger** internally uses a `BlockingCollection<LogMessage>` in a long running `Task` that decouples the logging from the working thread and prevents concurrent file access in scenarios where a lot of log messages have to be written in a short period of time.
