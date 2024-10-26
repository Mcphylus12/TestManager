using System.Collections.Generic;
using System.Reflection;
using TestManager.PluginLib;

namespace TestManager;

public static class HandlerScanner
{
    private static List<Assembly> _assemblies = new List<Assembly>();

    public static void AddAssembly(Assembly a) => _assemblies.Add(a);

    public static Dictionary<string, ITestHandler> GetHandlers()
    {
        var types = _assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(ITestHandler)))).ToArray();

        return types.Select(t =>
        {
            Console.WriteLine("Loading TestHandler: " + t.FullName);
            return (ITestHandler)Activator.CreateInstance(t)!;
        }).ToDictionary(t => t.Type, t => t);
    }

    internal static void LoadPlugins(DirectoryInfo info)
    {
        var pluginsFolder = new DirectoryInfo(Path.Combine(info.FullName, ".config", "plugins"));

        if (pluginsFolder.Exists)
        {
            //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var plugins = pluginsFolder.EnumerateFiles().Select(p => Assembly.LoadFrom(p.FullName));
            foreach (var item in plugins)
            {
                Console.WriteLine("Loading " + item.FullName);
                _assemblies.Add(item);
            }
        }
        else
        {
            Console.WriteLine("Skipping plugins");
        }

        //static Assembly CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        //{
        //    String DllName = new AssemblyName(args.Name).Name + ".dll";
        //    Console.WriteLine("Loading Dependency " + DllName);
        //    return Assembly.LoadFile(DllName);
        //}
    }
}
