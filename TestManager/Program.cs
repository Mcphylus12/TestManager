// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using Serilog;
using TestManager;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var root = new RootCommand();
var rootDirOption = new Option<DirectoryInfo>("--root");
rootDirOption.IsRequired = true;
rootDirOption.Description = "The root directory to run/manage tests from";
rootDirOption.AddAlias("-r");

var submitResultsOption = new Option<bool?>("--submit");
submitResultsOption.Description = "Whether to submit results via loaded results integrator";
submitResultsOption.AddAlias("-s");

var verbosityOption = new Option<string?>("--verbosity");
verbosityOption.IsRequired = false;
verbosityOption.Description = "Logging Verbosity";
verbosityOption.SetDefaultValue('i');
verbosityOption.AddAlias("-v");

root.AddGlobalOption(rootDirOption);
root.AddGlobalOption(submitResultsOption);
root.AddGlobalOption(verbosityOption);
root.AddCommand(ManageCommand.Build(rootDirOption, submitResultsOption, verbosityOption));
root.AddCommand(BulkCommand.Build(rootDirOption, submitResultsOption, verbosityOption));


await root.InvokeAsync(args);
