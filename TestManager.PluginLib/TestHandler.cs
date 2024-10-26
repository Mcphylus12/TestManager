namespace TestManager.PluginLib;
public interface ITestHandler
{
    public string Type { get; }
}

public interface ITestHandler<T> : ITestHandler
{
    Task RunTest(ITestParameters<T> testParameters, TestResult testResult);
}

public interface ITestParameters<T>
{
    T Parameters { get; set; }
    byte[]? File { get; set; }
    string FileName { get; set; }
    string TestName { get; set; }

    void EnsureFile();
    void EnsureFileName();
}

