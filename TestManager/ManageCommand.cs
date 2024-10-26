// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TestManager.Management;
using TestManager.PluginLib;

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
