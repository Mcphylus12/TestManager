// See https://aka.ms/new-console-template for more informatio
using System.CommandLine;
using TestManager;
using TestManager.FileTraversal;
using TestManager.Testing;

public class BulkCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir)
    {
        var option = new Option<string?>("--pattern", "pattern of files to run (if not provided all found tests will run)");
        option.AddAlias("-p");

        var command = new Command("bulk", "Run a bulk test");
        command.AddOption(option);
        command.SetHandler(Run, option, rootDir);

        return command;
    }

    public static async Task Run(string? pattern, DirectoryInfo rootDir)
    {
        var fileFinder = new FileFinder(rootDir);
        var testLoader = new TestLoader(HandlerScanner.GetHandlers(), rootDir);
        var testRunner = new TestRunner();
        var files = pattern is null ? fileFinder.GetAllTestFiles() : fileFinder.GetMatchingTestFiles(pattern);
        var tests = await testLoader.LoadTests(files);
        var result = await testRunner.RunTests(tests);
        Console.WriteLine(result.ToJson());
    }
}
