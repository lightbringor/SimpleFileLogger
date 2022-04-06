﻿using Dc.Ops.SimpleFileLogger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SimpleFileLogger.Example.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        var obj = new { a = "a", b = 2};
        _logger.LogDebug("OnGet {obj}", obj.ToJson(_logger, LogLevel.Debug));
    }
}
