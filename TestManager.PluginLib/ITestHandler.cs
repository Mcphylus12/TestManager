using System.Diagnostics.CodeAnalysis;

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
    T Parameters { get; }
    byte[]? File { get; }
    string FileName { get; }
    string TestName { get; }
    ISecretLoader SecretLoader { get; }

    [MemberNotNull(nameof(File))]
    void EnsureFile();
    void EnsureFileName();
}

