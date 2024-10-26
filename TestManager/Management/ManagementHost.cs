using System.Diagnostics;
using System.Net.Mime;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace TestManager.Management;
public static class ManagementHost
{
    public static async Task Run(ManagementSession managementSession)
    {
        var builder = WebApplication.CreateBuilder([]);

        builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

        var app = builder.Build();

        app.UseCors();
        var site = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Management/Index.html"));

        app.MapGet("/", () => Results.Content(site, MediaTypeNames.Text.Html));
        app.MapGet("/get-files", (string? path, string mode) => managementSession.GetFiles(mode, path));
        app.MapPost("/run", (string file) => managementSession.Run(file));
        app.MapPost("/bulkrun", (string pattern) => managementSession.BulkRun(pattern));
        app.MapGet("/load", (string file) => managementSession.Load(file));
        app.MapPost("/save", (string file, [FromBody] HandlerForm[] data) => managementSession.Save(file, data));



        await app.RunAsync();
    }
}
