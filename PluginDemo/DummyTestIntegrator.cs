using TestManager.PluginLib;

namespace PluginDemo;
public class DummyTestIntegrator : ITestResultIntegrator
{
    private Dictionary<string, string>? _config;
    private ISecretLoader _secretLoader;

    public DummyTestIntegrator(Dictionary<string, string>? config, ISecretLoader secretLoader)
    {
        _config = config;
        _secretLoader = secretLoader;
    }

    public ISet<string> FieldDefinitions { get; } = new HashSet<string>
    {
        "Id",
        "Description",
        "Description2"
    };

    public static ITestResultIntegrator Create(Dictionary<string, string>? config, ISecretLoader secretLoader)
    {
        return new DummyTestIntegrator(config, secretLoader);
    }

    public Task SubmitResults(IEnumerable<TestResult> results)
    {
        var secret = _secretLoader.LoadSecret("DummySecret");

        Console.WriteLine("Secret Loaded: " + secret);

        Console.WriteLine("INTEGRATION RESULTS -- START");
        foreach (var item in results.SelectMany(t => t.Assertions))
        {
            Console.WriteLine(item.ToString());
        }
        Console.WriteLine("INTEGRATION RESULTS -- END");
        return Task.CompletedTask;
    }
}
