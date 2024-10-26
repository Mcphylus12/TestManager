using TestManager.PluginLib;

namespace TestManager.Handlers;
internal class LengthTestHandler : ITestHandler<LengthData>
{
    public string Type => "FileLength";

    public Task RunTest(ITestParameters<LengthData> testParameters, TestResult testResult)
    {
        testParameters.EnsureFile();

        testResult.AddResult("length_check", testParameters.Parameters.ExpectedLength, testParameters.File.LongLength);
        return Task.CompletedTask;
    }
}

internal class LengthData
{
    public long ExpectedLength { get; set; }
}
