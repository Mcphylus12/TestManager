// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Reflection;
using TestManager;

HandlerScanner.AddAssembly(Assembly.GetEntryAssembly()!);

var root = new RootCommand();
var rootDirOption = new Option<DirectoryInfo>("--root");
rootDirOption.IsRequired = true;
rootDirOption.Description = "The root directory to run/manage tests from";
rootDirOption.AddAlias("-r");
root.AddGlobalOption(rootDirOption);
root.AddCommand(ManageCommand.Build(rootDirOption));
root.AddCommand(BulkCommand.Build(rootDirOption));


await root.InvokeAsync(args);
