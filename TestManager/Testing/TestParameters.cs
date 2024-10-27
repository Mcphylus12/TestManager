using System.Diagnostics.CodeAnalysis;
using TestManager.PluginLib;

namespace TestManager.Testing;

internal class TestParameters<T> : ITestParameters<T>
{
    private TestLoader.FileLoader _fileLoader;

    public TestParameters(TestLoader.FileLoader fileLoader)
    {
        _fileLoader = fileLoader;
    }

    public byte[]? File => _fileLoader.Load(FileName);
    public required ISecretLoader SecretLoader { get; init; }
    public required string FileName { get; init; }
    public required string TestName { get; init; }
    public required T Parameters { get; init; }

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
}
