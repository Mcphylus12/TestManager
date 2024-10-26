using System.Security.Cryptography;
using TestManager.PluginLib;

namespace TestManager.Handlers;
internal class Sha256TestHandler : ITestHandler<HashConfig>
{
    public string Type => "SHA256";

    public Task RunTest(ITestParameters<HashConfig> testParameters, TestResult testResult)
    {
        testParameters.EnsureFile();
        var hash = SHA256.HashData(testParameters.File!);
        var hashString = Convert.ToHexString(hash);
        testResult.AddResult("hashes_match", testParameters.Parameters.ExpectedHash, hashString);
        return Task.CompletedTask;
    }
}

internal class HashConfig
{
    public required string ExpectedHash { get; set; }
}
