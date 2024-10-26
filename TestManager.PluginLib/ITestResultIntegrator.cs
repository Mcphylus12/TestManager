namespace TestManager.PluginLib;
public interface ITestResultIntegrator
{
    void Configure(Dictionary<string, string> config);

    ISet<string> FieldDefinitions { get; }
    Task SubmitResults(IEnumerable<TestResult> results);
}
