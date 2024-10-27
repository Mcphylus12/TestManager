using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TestManager.FileTraversal;

namespace TestManager.Management;
public static class ManagementHost
{
    public static async Task Run(ManagementSession managementSession)
    {
        Log.Information("Running on http://localhost:8080");
        var builder = WebApplication.CreateBuilder([]);
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new StringLoggerProvider());
        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
        builder.WebHost.UseSetting(WebHostDefaults.HttpPortsKey, "8080");
        var app = builder.Build();

        app.UseCors();
        var site = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Management/Index.html"));
        app.MapGet("/", () => Results.Content(site, MediaTypeNames.Text.Html));
        app.MapGet("/get-files", (string? path, string mode) => managementSession.GetFiles(mode, path));
        app.MapPost("/run", (string file) => managementSession.Run(file));
        app.MapPost("/bulkrun", (string pattern) => managementSession.BulkRun(pattern));
        app.MapGet("/load", (string file) => managementSession.Load(file));
        app.MapPost("/save", (string file, [FromBody] HandlerForm[] data) => managementSession.Save(file, data));
        app.MapDelete("/testfile", (string file) => managementSession.DeleteTestFile(file));
        app.MapPost("/testfile", ([FromBody] Entry newFile) => managementSession.CreateTestFile(newFile) ? Results.Ok() : Results.Conflict());

        await app.RunAsync();
    }
}

internal class StringLoggerProvider : ILoggerProvider
{
    public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
    {
        return new StringLogger(categoryName);
    }

    public void Dispose()
    {
        // no op
    }

    private class StringLogger : Microsoft.Extensions.Logging.ILogger
    {
        private string _categoryName;

        public StringLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Serilog.Log.Debug(message);
        }
    }
}
