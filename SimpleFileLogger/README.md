# Simple File Logger

Lightweight library for writing log information to files using the .NET `ILogger<T>` interface. For more information regarding the usage of `ILogger<T>` see the [Microsoft Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0)

## Quick Start

**The minimum required framework version is `.NET 6`**

The library is available as a [NuGet package](https://www.nuget.org/packages/Lightbringer.SimpleFileLogger/) named 
`Lightbringer.SimpleFileLogger`.

After including the package in your project, SimpleFileLogger exposes an extension method
to add file logging with only one line of code during startup:

```csharp
using SimpleFileLogger;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSimpleFileLogging(builder.Configuration);
```

With only a minimum of configuration you can start logging to `./Logs/Default_yyyy-mm-dd.log` immediately:

```json
    "Logging": { 
        "LogLevel": {
            // Define LogLevels here ...
        },
        "FileLoggerOptions": {
            "LogFolder": "./Logs",
            "FileNamesWithoutExtension": {
                "*": "Default"
            }
        }
    }
```

## Detailed Setup

If your file logging options are not located under `Logging:FileLoggerOptions` in `appsettings.json`, you can pass the section as a second parameter:

```csharp
builder.Services.AddSimpleFileLogging(builder.Configuration, "MyFileLoggingConfigSection");
```

If you prefer to provide the file logging options via code instead of `appsettings.json`, use the traditional way of configuration:

```csharp
builder.Services.AddLogging(logBuilder =>
{
    logBuilder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();
    logBuilder.Services.Configure<LoggerOptions>(options => ConfigureFileLogging(options));
});

private void ConfigureFileLogging(LoggerOptions options)
{
    options.LogFolder = "./Logs";
    // ...
}
```

### Configuration

Configuration should usually be provided via `appsettings.json` in section `Logging:FileLoggerOptions`

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
                "MyNamespace.MyClass": "MyClass",
                "*": "Default"
            },
            "NumberOfDaysToKeepLogs": 5,
            "EventOptions": [
                { "Id": 99, "SubFolder": "Errors" },
                { "Id": 1, "SubFolder": "Sub", "NameExtension": "AddName" },
                { "Id": 2, "SubFolderFromEventName": true },
                { "Id": 3, "NameExtensionFromEventName": true }
            ]
        }
    }
```
In the `LogLevel` section, the minimum level that leads to writing a log message to a file can be specified for namespaces or classes using the full qualified name. `Default` applies to all classes in namespaces not mentioned in the configuration. This is the .NET logging standard and nothing specific of *SimpleFileLogger*.

In the section below named `FileLoggerOptions` the options for file logging are provided. You can specify a root folder with the `LogFolder` value. If it is not set, the directory of the application assembly is used as root. Then a file name **without extension** can be specified for each namespace or class that shall be logged in its own file. The path is relative to `LogFolder`. The entry `*` is mandatory and specifies the default log file used for any content that falls not in one of the namespace defined above.

The file names provided here are extended by `_yyyy-MM-dd.log` (as long as no further `EventOptions` are provided, see below). 
So a new log file is created every day.

**New in v1.1.3**

**With the property `NumberOfDaysToKeepLogs` it can now be controlled how long log files will be stored. If this property
is defined and has a value > 0, a method that checks for outdated log files will run on startup and then every hour.
All `*.log` files within your defined `LogFolder` that are older than the number of defined days will be deleted.**

To get more control over file names and log directories, certain options can be provided via the `EventOptions` array. The `EventId` provided for a call to `ILogger.Log(eventId, ...)` will be matched against the specified `EventOptions` objects.
The follwing properties are available:

- `Id`: `int` **mandatory** , must match the `EventId.Id` property provided for logging
- `SubFolder`: `string` **optional** , a folder that will be inserted between the default `LogFolder` and the `FileName`. Can contain multiple directories
  - e.g. `sub/path`
- `NameExtension`: `string` **optional**, an addition that will be applied after the defined `FileName` and before the date.
  - e.g. `_nameExtension` --> `fileName_nameExtension_22-04-02.log`  (**note that the `_` here is part of the definition and will not be inserted automatically**) 
- `SubFolderFromEventName`: `boolean` **optional**, if set to `true`, the sub folder to insert between `LogFolder` and the `FileName`  will be taken dynamically from `EventId.Name` when calling `ILogger.Log(eventId, ...)`. If `EventId.Name` is `null` or empty, the `Id` number will be used
- `NameExtensionFromEventName`: `boolean` **optional**, if set to `true`, the exentsion applied to the `FileName` will be taken dynamically from `EventId.Name` when calling `ILogger.Log(eventId, ...)`. If `EventId.Name` is `null` or empty, the `Id` number will be used. **Here, an `_` will be inserted automatically between default file name and exension**

`LogLevel`s can be changed at any time without restarting the application. Changes in file names only apply after a restart.

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
            // assuming that an EventOption definiton is set like {Id: 99, SubFolder: "exceptions"}
            // the message goes into e.g. logs/exceptions/MyClass_2022-04-08.log
            logger.LogCritical(99, exc, "Exception!");
        }
    }

    public void ChangeObject(Foo obj)
    {
        obj.Name = "bar";
        var eventId = new EventId(10, obj.Name)
        // ...
        // assuming that an EventOption definiton is set like {Id: 10, NameExtensionFromEventName: true}
        // the message goes into e.g. logs/MyClass_bar_2022-04-08.log
        logger.LogDebug(eventId, "Value set for {val}", obj.Value)
    }
```

### Logging of complex objects

The library provides an extension method `ToJson(ILogger? logger = null, LogLevel level = LogLevel.None)` for `object` that serializes the object to `JSON`. Reference cycles are not serialized. If the serialization could be expensive, you
can provide the `ILogger` instance and the `LogLevel` for the `Log(...)` call to prevent unnecessary serialization if logging
is currently not enabled for the defined level.

```csharp
// serialization will always be performed
logger.LogError(exc, "Operation failed! Resource was:\n{resourceJson}", newResource.ToJson());

// serialization will only be performed if Trace logging is currently enabled for the logger instance
logger.LogTrace("Trace logging! largeObject is:\n{objJson}", largeObject.ToJson(logger, LogLevel.Trace));
```

**Be carful with frequent logging of complex objects with a lot of references to other complex objects because the log size could increase rapidly. Advice is to do this only for a limited time when Trace or Debug logging is enabled.**

<mark>**Note that Microsoft recommends to not us `$"{var} text"` interpolation but to use placeholders and parameters that are passed to the log method.**</mark>

## Delete Old Log Files

## More



**SimpleFileLogger** internally uses a `BlockingCollection<LogMessage>` in a long running `Task` that decouples the logging from the working thread and prevents concurrent file access in scenarios where a lot of log messages have to be written in a short period of time.
