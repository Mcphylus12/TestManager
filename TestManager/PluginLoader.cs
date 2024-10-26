using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using TestManager.PluginLib;

namespace TestManager;

public class PluginLoader
{
    private static List<Assembly> _assemblies = new List<Assembly>();
    private readonly DirectoryInfo _directoryInfo;
    private readonly ISecretLoader _secretLoader;

    public PluginLoader(DirectoryInfo root, ISecretLoader secretLoader)
    {
        AddAssembly(Assembly.GetEntryAssembly()!);
        _directoryInfo = root;
        _secretLoader = secretLoader;
        LoadPlugins(root);
    }

    public void AddAssembly(Assembly a) => _assemblies.Add(a);

    public Dictionary<string, ITestHandler> GetHandlers()
    {
        var types = _assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(ITestHandler)))).ToArray();

        return types.Select(t =>
        {
            Console.WriteLine("Loading TestHandler: " + t.FullName);
            return (ITestHandler)Activator.CreateInstance(t)!;
        }).ToDictionary(t => t.Type, t => t);
    }


    public bool GetIntegrator([NotNullWhen(true)] out ITestResultIntegrator? integrator)
    {
        var types = _assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(ITestResultIntegrator)))).ToArray();

        var config = LoadIntegratorConfig();

        integrator = types.Length switch
        {
            0 => null,
            1 => ITestResultIntegrator.Create(types[0], config, _secretLoader),
            _ => throw new NotSupportedException("Multiple test result integrators are not supported as plugins")
        };

        return integrator != null;
    }

    private Dictionary<string, string>? LoadIntegratorConfig()
    {
        var config = new FileInfo(Path.Combine(_directoryInfo.FullName, ".config", "integrator.json"));

        if (config.Exists)
        {
            var rawConfig = File.ReadAllText(config.FullName);

            return JsonSerializer.Deserialize<Dictionary<string, string>>(rawConfig)!;
        }

        return null;
    }

    private void LoadPlugins(DirectoryInfo info)
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
