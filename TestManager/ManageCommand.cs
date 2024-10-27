// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using Serilog;
using Serilog.Events;
using TestManager.Management;
using TestManager.PluginLib;

namespace TestManager;

public class ManageCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir, Option<bool?> submitResultsOption, Option<string?> verbosityOption)
    {
        var command = new Command("manage", "Manage test");
        command.SetHandler(Run, rootDir, submitResultsOption, verbosityOption);

        return command;
    }

    private static async Task Run(DirectoryInfo info, bool? submit, string? verbosity)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(verbosity switch
            {
                "d" => LogEventLevel.Debug,
                "i" or null => LogEventLevel.Information,
                _ => throw new NotSupportedException("Invalid verbosity switch")
            })
            .WriteTo.Console()
            .CreateLogger();

        var secretLoader = new SecretLoader();
        var loader = new PluginLoader(info, secretLoader);

        ITestResultIntegrator? integrator = null;
        if (submit.GetValueOrDefault())
        {
            loader.GetIntegrator(out integrator);
        }

        var manSession = new ManagementSession(info, loader.GetHandlers(), integrator, new SecretLoader());
        await ManagementHost.Run(manSession);
    }
}
