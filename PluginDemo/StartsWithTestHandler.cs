using System.Text;
using TestManager.PluginLib;

namespace PluginDemo;
public class StartsWithTestHandler : ITestHandler<StartWithConfig>
{
    public string Type => "StartsWith";

    public Task RunTest(ITestParameters<StartWithConfig> testParameters, TestResult testResult)
    {
        var fileText = Encoding.UTF8.GetString(testParameters.File);

        testResult.AddResult("main", fileText.StartsWith(testParameters.Parameters.Text), message: "custom message i could populate with useful info in the even of failure");

        return Task.CompletedTask;
    }
}

public class StartWithConfig
{
    public string Text { get; set; }
}
