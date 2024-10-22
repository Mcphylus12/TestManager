// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using TestManager.Management;

namespace TestManager;

public class ManageCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir)
    {
        var command = new Command("manage", "Manage test");
        command.SetHandler(Run, rootDir);

        return command;
    }

    private static async Task Run(DirectoryInfo info)
    {
        var manSession = new ManagementSession(info, HandlerScanner.GetHandlers());
        await ManagementHost.Run(manSession);
    }
}
