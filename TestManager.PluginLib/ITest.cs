namespace TestManager.PluginLib;
public interface ITest
{
    string? FilePath { get; set; }
    string? TestName { get; set; }
    string? TestFilePath { get; set; }
    Dictionary<string, string>? IntegrationParameters { get; set; }
    Task<TestResult> Run();
}
