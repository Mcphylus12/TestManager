using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Toolkit.FileTester;

namespace TestManager.Testing;
public interface ITestHandler
{
    public string Type { get; }
}

public interface ITestHandler<T> : ITestHandler
{
    Task RunTest(TestParameters<T> testParameters, TestResult testResult);
}

public class TestParameters<T> : ITestParameters
{
    public byte[]? File { get; set; }
    public required string FileName { get; set; }
    public required string TestName { get; set; }
    public required T Parameters { get; set; }

    [MemberNotNull(nameof(File))]
    public void EnsureFile()
    {
        if (File is null || File.Length == 0)
        {
            throw new TestException("Test Requires a loaded file");
        }
    }

    [MemberNotNull(nameof(FileName))]
    public void EnsureFileName()
    {
        if (FileName is null || FileName.Length == 0)
        {
            throw new TestException("Test Requires a filename");
        }
    }

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions() { NumberHandling = JsonNumberHandling.AllowReadingFromString };
    public void Setup(TsJsonContract s, int i, string testPath, string? filePath, string adjacentFile, FileLoader fl)
    {
        FileName = filePath ?? adjacentFile;
        Parameters = s.Parameters.Deserialize<T>(_options)!;
        TestName = s.Name ?? $"{testPath}:{i}";
        File = fl.Load(filePath ?? adjacentFile);
    }
}

public interface ITestParameters
{
    void Setup(TsJsonContract s, int i, string testPath, string? filePath, string adjacentFile, FileLoader fl);
}
