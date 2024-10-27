using System.Text.Json;
using TestManager.PluginLib;

namespace TestManager.Testing;

internal class TestRunner
{
    internal async Task<RunResult> RunTests(IEnumerable<ITest> tests)
    {
        var results = new List<TestResult>();
        foreach (var test in tests)
        {
            TestResult? result = null;
            try
            {
                result = await test.Run();
            }
            catch (Exception ex)
            {
                result?.AddResult("unhandled_exception", false, ex.ToString());
            }
            finally
            {
                if (result is not null)
                {
                    results.Add(result);
                }
            }
        }

        return new RunResult(results);
    }
}
