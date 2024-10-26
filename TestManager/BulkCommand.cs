// See https://aka.ms/new-console-template for more informatio
using System.CommandLine;
using TestManager;
using TestManager.FileTraversal;
using TestManager.Testing;

public class BulkCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir, Option<bool?> submitResultsOption)
    {
        var option = new Option<string?>("--pattern", "pattern of files to run (if not provided all found tests will run)");
        option.AddAlias("-p");

        var command = new Command("bulk", "Run a bulk test");
        command.AddOption(option);
        command.SetHandler(Run, option, rootDir, submitResultsOption);

        return command;
    }

    public static async Task Run(string? pattern, DirectoryInfo rootDir, bool? submit)
    {
        var loader = new PluginLoader(rootDir);
        var fileFinder = new FileFinder(rootDir);
        var testLoader = new TestLoader(loader.GetHandlers(), rootDir);
        var testRunner = new TestRunner();
        var files = pattern is null ? fileFinder.GetAllTestFiles() : fileFinder.GetMatchingTestFiles(pattern);
        var tests = await testLoader.LoadTests(files);
        var result = await testRunner.RunTests(tests);

        if (submit.GetValueOrDefault() && loader.GetIntegrator(out var integrator))
        {
            await integrator.SubmitResults(result.TestResults);
        }

        Console.WriteLine(result.ToJson());
    }
}
