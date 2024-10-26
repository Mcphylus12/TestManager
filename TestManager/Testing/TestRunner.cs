using System.Text.Json;
using TestManager.PluginLib;
using static TestManager.PluginLib.TestResult;

namespace TestManager.Testing;

internal class TestRunner
{
    internal async Task<RunResult> RunTests(IEnumerable<ITest> tests)
    {
        var results = new List<TestResult>();
        foreach (var test in tests)
        {
            var result = new TestResult(test.Name);
            try
            {
                await test.Run(result);
            }
            catch (Exception ex)
            {
                result.AddResult("unhandled_exception", false, ex.ToString());
            }
            finally
            {
                results.Add(result);
            }
        }

        return new RunResult(results);
    }
}

public class RunResult
{
    private readonly List<TestResult> _results;

    public RunResult(List<TestResult> results)
    {
        _results = results;
    }

    public IEnumerable<TestResult> TestResults => _results;

    public IEnumerable<AssertionResult> Enumerate()
    {
        return _results.SelectMany(r => r.Assertions);
    }

    public string? ToJson()
    {
        return JsonSerializer.Serialize(_results);
    }
}
