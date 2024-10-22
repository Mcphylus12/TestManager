using System.Reflection;
using TestManager.Testing;

namespace TestManager;

public static class HandlerScanner
{
    private static List<Assembly> _assemblies = new List<Assembly>();

    public static void AddAssembly(Assembly a) => _assemblies.Add(a);

    public static Dictionary<string, ITestHandler> GetHandlers()
    {
        var types = _assemblies.SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsAssignableTo(typeof(ITestHandler)))).ToArray();

        return types.Select(t => (ITestHandler)Activator.CreateInstance(t)!).ToDictionary(t => t.Type, t => t);
    }
}
