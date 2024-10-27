// See https://aka.ms/new-console-template for more informatio
using System.CommandLine;
using Serilog.Events;
using Serilog;
using TestManager;
using TestManager.FileTraversal;
using TestManager.Testing;

public class BulkCommand
{
    public static Command Build(Option<DirectoryInfo> rootDir, Option<bool?> submitResultsOption, Option<string?> verbosityOption)
    {
        var option = new Option<string?>("--pattern", "pattern of files to run (if not provided all found tests will run)");
        option.AddAlias("-p");

        var command = new Command("bulk", "Run a bulk test");
        command.AddOption(option);
        command.SetHandler(Run, option, rootDir, submitResultsOption, verbosityOption);

        return command;
    }

    public static async Task Run(string? pattern, DirectoryInfo rootDir, bool? submit, string? verbosity)
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
        var pluginLoader = new PluginLoader(rootDir, secretLoader);
        var fileFinder = new FileFinder(rootDir);
        var testLoader = new TestLoader(pluginLoader.GetHandlers(), rootDir, secretLoader);
        var testRunner = new TestRunner();
        var files = pattern is null ? fileFinder.GetAllTestFiles() : fileFinder.GetMatchingTestFiles(pattern);
        var tests = await testLoader.LoadTests(files);
        var result = await testRunner.RunTests(tests);

        if (submit.GetValueOrDefault() && pluginLoader.GetIntegrator(out var integrator))
        {
            await integrator.SubmitResults(result);
        }

        Log.Information(result.ToJson()!);
    }
}
