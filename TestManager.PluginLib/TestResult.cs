using System.Text.Json;

namespace TestManager.PluginLib;


public class TestResult
{
    public ITest Test { get; }

    public List<AssertionResult> Assertions { get; }

    public TestResult(ITest test)
    {
        Assertions = new List<AssertionResult>();
        Test = test;
    }

    public TestResult AddResult(string assertionName, bool isSuccess, string? message = null)
    {
        Assertions.Add(new AssertionResult(assertionName, isSuccess, message));
        return this;
    }

    public record class AssertionResult(string AssertionName, bool IsSuccess, string? Message);
}

public static class ResultExtensions
{
    public static TestResult AddResult(this TestResult result, string assertionName, object expected, object actual)
    {
        var success = expected == actual;
        var message = success ? null : $"Actual was: {actual}";

        return result.AddResult(assertionName, success, message);
    }

    public static TestResult AddResult<T>(this TestResult result, string assertionName, T expected, T actual)
    where T : IEquatable<T>
    {
        var success = expected.Equals(actual);
        var message = success ? null : $"Actual was: {actual}";

        return result.AddResult(assertionName, success, message);
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

    public string? ToJson()
    {
        return JsonSerializer.Serialize(_results);
    }

    public IEnumerable<string> ToConsoleOutput()
    {
        foreach (var (test, assertion) in _results.SelectMany(i => i.Assertions, (test, assertion) => (test, assertion)))
        {
            yield return $"{test.Test.TestName} - {assertion.AssertionName} {(assertion.IsSuccess ? "PASSED" : "FAILED")} {assertion.Message}";
        }
    }
}
