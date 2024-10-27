using TestManager.PluginLib;

namespace TestManager.Testing;

internal class TestLoadException : Exception
{
    private FileInfo _file;
    private Exception _ex;
    private TsJsonContract? _testConfiguration;
    private int? _testNumberInFile;

    public TestLoadException(FileInfo file, Exception ex)
    {
        _file = file;
        _ex = ex;
    }

    public TestLoadException(FileInfo testFile, TsJsonContract testConfiguration, int testNumberInFile, Exception ex)
    {
        _file = testFile;
        _testConfiguration = testConfiguration;
        _testNumberInFile = testNumberInFile;
        _ex = ex;
    }

    internal ITest AsTest()
    {
        return new ExceptedTest(_ex)
        {
            TestFilePath = _file.FullName,
            TestName = _testConfiguration?.Name ?? "failed_testload_placeholder_name",
            IntegrationParameters = null,
            FilePath = null
        };
    }
}

internal class ExceptedTest : ITest
{
    private Exception _exception;

    public ExceptedTest(Exception innerException)
    {
        _exception = innerException;
    }

    public string? FilePath { get; set; }
    public string? TestName { get; set; }
    public string? TestFilePath { get; set; }
    public Dictionary<string, string>? IntegrationParameters { get; set; }

    public Task<TestResult> Run()
    {
        var result = new TestResult(this);
        result.AddResult("failed_test_load", false, _exception!.ToString());
        return Task.FromResult(result);
    }
}
