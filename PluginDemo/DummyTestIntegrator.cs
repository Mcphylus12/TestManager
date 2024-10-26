using TestManager.PluginLib;

namespace PluginDemo;
public class DummyTestIntegrator : ITestResultIntegrator
{
    public ISet<string> FieldDefinitions { get; } = new HashSet<string>
    {
        "Id",
        "Description",
        "Description2"
    };

    public void Configure(Dictionary<string, string> config)
    {
        Console.WriteLine("Test Config Value = " + config["configField"]);
    }

    public Task SubmitResults(IEnumerable<TestResult> results)
    {
        Console.WriteLine("INTEGRATION RESULTS -- START");
        foreach (var item in results.SelectMany(t => t.Assertions))
        {
            Console.WriteLine(item.ToString());
        }
        Console.WriteLine("INTEGRATION RESULTS -- END");
        return Task.CompletedTask;
    }
}
