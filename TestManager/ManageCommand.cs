// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TestManager.Management;
using TestManager.PluginLib;
using TestManager.Testing;

namespace TestManager;

public class ManageCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir, Option<bool?> submitResultsOption)
    {
        var command = new Command("manage", "Manage test");
        command.SetHandler(Run, rootDir, submitResultsOption);

        return command;
    }

    private static async Task Run(DirectoryInfo info, bool? submit)
    {
        var loader = new PluginLoader(info);

        ITestResultIntegrator? integrator = null;
        if (submit.GetValueOrDefault())
        {
            loader.GetIntegrator(out integrator);
        }

        var manSession = new ManagementSession(info, loader.GetHandlers(), integrator);
        await ManagementHost.Run(manSession);
    }
}
