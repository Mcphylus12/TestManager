using System.Text.Json;
using Toolkit.FileTester;
using static TestManager.Testing.TestResult;

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

public class TestResult
{
    private readonly string _testName;
    public List<AssertionResult> Assertions { get; }

    public TestResult(string testName)
    {
        Assertions = new List<AssertionResult>();
        _testName = testName;
    }

    public TestResult AddResult(string assertionName, bool isSuccess, string? message = null)
    {
        Assertions.Add(new AssertionResult(_testName, assertionName, isSuccess, message));
        return this;
    }

    public record class AssertionResult(string TestName, string AssertionName, bool IsSuccess, string? Message);
}

public class RunResult
{
    private readonly List<TestResult> _results;

    public RunResult(List<TestResult> results)
    {
        _results = results;
    }

    public IEnumerable<AssertionResult> Enumerate()
    {
        return _results.SelectMany(r => r.Assertions);
    }

    public string? ToJson()
    {
        return JsonSerializer.Serialize(_results);
    }
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