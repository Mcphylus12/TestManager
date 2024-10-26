using PluginDep;
using TestManager.PluginLib;

namespace PluginDemo;

public class JsonTestHandler : ITestHandler<NoParams>
{
    public string Type => "JSON";

    public Task RunTest(ITestParameters<NoParams> testParameters, TestResult testResult)
    {
        testParameters.EnsureFile();

        try
        {
            var output = JsonParser.Parse(testParameters.File);
            testResult.AddResult("check", output is not null);
        }
        catch (Exception ex)
        {
            testResult.AddResult("check", false, ex.Message);
        }
        return Task.CompletedTask;
    }
}

public class NoParams
{
}
