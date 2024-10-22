using System.Reflection;
using FluentAssertions;
using TestManager;
using TestManager.Testing;

namespace Tests;

public class BasicTests
{
    [Fact]
    public async Task BulkTestWorks()
    {
        HandlerScanner.AddAssembly(Assembly.GetAssembly(typeof(DummyHandler))!);
        using var tw = new StringWriter();
        var ogOut = Console.Out;
        Console.SetOut(tw);
        await BulkCommand.Run("*", new DirectoryInfo("data"));

        var output = tw.ToString();

        output.Should().NotBeEmpty();
        output.Should().Contain("msg hello");
        Console.SetOut(ogOut);
    }
}

public class DummyHandler : ITestHandler<DummyConfig>
{
    public string Type => "dummy";

    public Task RunTest(TestParameters<DummyConfig> testParameters, TestResult testResult)
    {
        testResult.AddResult("dummy", true, $"msg {testParameters.Parameters.DummyValue}");
        return Task.CompletedTask;
    }
}

public class DummyConfig
{
    public required string DummyValue { get; set; }
}
