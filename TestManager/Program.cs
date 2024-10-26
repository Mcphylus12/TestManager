// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Reflection;
using TestManager;

var root = new RootCommand();
var rootDirOption = new Option<DirectoryInfo>("--root");
rootDirOption.IsRequired = true;
rootDirOption.Description = "The root directory to run/manage tests from";
rootDirOption.AddAlias("-r");

var submitResultsOption = new Option<bool?>("--submit");
submitResultsOption.Description = "Whether to submit results via loaded results integrator";
submitResultsOption.AddAlias("-s");

root.AddGlobalOption(rootDirOption);
root.AddGlobalOption(submitResultsOption);
root.AddCommand(ManageCommand.Build(rootDirOption, submitResultsOption));
root.AddCommand(BulkCommand.Build(rootDirOption, submitResultsOption));


await root.InvokeAsync(args);
