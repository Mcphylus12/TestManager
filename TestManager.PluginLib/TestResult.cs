namespace TestManager.PluginLib;


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
