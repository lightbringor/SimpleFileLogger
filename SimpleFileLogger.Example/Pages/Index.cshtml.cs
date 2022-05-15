using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleFileLogger.Example.Model;
using SimpleFileLogger;

namespace SimpleFileLogger.Example.Pages;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public int EventId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EventName { get; set; }

    public string? LogConfigString { get; set; }

    public string? LogResult { get; set; }
    public bool Error { get; private set; }

    private readonly ILogger<IndexModel> logger;
    private bool logged = false;
    private string currentAwaitingLogGuid = Guid.NewGuid().ToString();
    private readonly ILogger generalLogger;

    public IndexModel(ILogger<IndexModel> logger, ILoggerProvider loggerProvider, IConfiguration configuration)
    {     
        generalLogger = loggerProvider.CreateLogger("GeneralLogger");
        var fileLoggerProvider = (loggerProvider as IFileLoggerProvider)!;
        fileLoggerProvider.MessageLogged += MessageLogged;
        this.logger = logger;

        var logConfig = new LogConfig();
        configuration.GetSection("Logging")?.Bind(logConfig);
        LogConfigString = logConfig.ToJson();
    }

    public void OnGet()
    {
        var obj = new { a = "a", b = 2 };
        var eventId = new EventId(1, "additionalName");
        logger.LogDebug(3, "OnGet {obj}|{guid}", obj.ToJson(logger, LogLevel.Debug), currentAwaitingLogGuid);
        WaitForWriteLogEntry();

    }

    public void OnPostLogTrace()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogTrace(eventId, "Test Trace Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();

    }

    public void OnPostLogDebug()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogDebug(eventId, "Test Debug Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();

    }

    public void OnPostLogInformation()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogInformation(eventId, "Test Information Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();
    }

    public void OnPostLogWarning()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogWarning(eventId, "Test Warning Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();
    }

    public void OnPostLogError()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogError(EventId, "Test Error Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();
    }

    public void OnPostLogCritical()
    {
        var eventId = new EventId(EventId, EventName);
        logger.LogCritical(eventId, "Test Critical Logging for Id={id}|{guid}", EventId, currentAwaitingLogGuid);
        WaitForWriteLogEntry();
    }

    public void OnPostLogToIndividualLog()
    {
        generalLogger.LogInformation("individualSub/individualLog", 0, null, $"my {{0}} text, {{1}}|{currentAwaitingLogGuid}", "interpolated", 5);
        WaitForWriteLogEntry();
    }


    /// <summary>
    /// Waits until either private filed logged is true (set in MessageLogged event handler) or 2 seconds pass, 
    /// which will indicate that something went wrong.
    /// </summary>
    private void WaitForWriteLogEntry()
    {
        var completedTaskIndex = Task.WaitAny(new[]{
            Task.Factory.StartNew( () =>
            {
                while (!logged)
                {
                    Task.Delay(50).Wait();
                }
            }),
            Task.Delay(2000)
        });

        if (completedTaskIndex == 1)
        {
            Error = true;
        }
    }

    private void MessageLogged(object? sender, MessageLoggedEventArgs e)
    {
        if (e.LogMessage.Content.Contains(currentAwaitingLogGuid))
        {
            LogResult = $"Logged '{e.LogMessage.Content.Replace($"|{currentAwaitingLogGuid}", "")}' to '{e.LogMessage.FullFilePath}'";
            logged = true;
        }

    }
}
