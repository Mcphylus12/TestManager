using TestManager.PluginLib;
using static System.Net.Mime.MediaTypeNames;

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
        "TestCaseId",
    };

    public static ITestResultIntegrator Create(Dictionary<string, string>? config, ISecretLoader secretLoader)
    {
        return new DummyTestIntegrator(config, secretLoader);
    }

    public Task SubmitResults(RunResult results)
    {
        var secret = _secretLoader.LoadSecret("DummySecret");

        Console.WriteLine("Secret Loaded: " + secret);

        Console.WriteLine("INTEGRATION RESULTS -- START");
        foreach (var (test, assertion) in results.TestResults.SelectMany(i => i.Assertions, (test, assertion) => (test, assertion)))
        {
            Console.WriteLine($"[{test.Test.IntegrationParameters?.GetValueOrDefault("TestCaseId")?.ToString()}]{test.Test.TestName} - {assertion.AssertionName} PASSED:{assertion.IsSuccess} {assertion.Message}");
        }
        Console.WriteLine("INTEGRATION RESULTS -- END");

        Console.WriteLine(results.ToJson());
        return Task.CompletedTask;
    }
}
