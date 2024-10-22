using System.Text.Json;

namespace TestManager.Testing;
internal class TestLoader
{
    private readonly Dictionary<string, ITestHandler> _handlers;
    private readonly DirectoryInfo _root;
    private readonly FileLoader _loader;

    public TestLoader(Dictionary<string, ITestHandler> handlers, DirectoryInfo root)
    {
        _handlers = handlers;
        _root = root;
        _loader = new FileLoader();
    }

    public async Task<IEnumerable<ITest>> LoadTests(IEnumerable<FileInfo> testFiles)
    {
        var tests = new List<ITest>();
        foreach (var file in testFiles)
        {
            var testConfigurations = await LoadTestFile(file);
            var adjacentFile = file.FullName[0..^7];

            int i = 1;
            foreach (var item in testConfigurations)
            {
                tests.Add(CreateRunner(item, i, adjacentFile));
                i++;
            }
        }

        return tests;
    }

    public static async Task<List<TsJsonContract>> LoadTestFile(FileInfo file)
    {
        var fileContents = await File.ReadAllTextAsync(file.FullName);
        var testJson = JsonDocument.Parse(fileContents);
        var testConfigurations = LoadTests(testJson);
        return testConfigurations;
    }

    private static List<TsJsonContract> LoadTests(JsonDocument testJson)
    {
        List<TsJsonContract> tests = new List<TsJsonContract>();

        try
        {
            if (testJson.RootElement.ValueKind == JsonValueKind.Object)
            {
                tests.Add(testJson.Deserialize<TsJsonContract>()!);
            }
            else if (testJson.RootElement.ValueKind == JsonValueKind.Array)
            {
                tests.AddRange(testJson.Deserialize<IEnumerable<TsJsonContract>>()!);
            }
        }
        catch (Exception ex)
        {
            // TODO: results.Add(new TestResult(file).AddResult("file_load_error", false, ex.ToString()));
        }

        return tests;
    }

    private ITest CreateRunner(TsJsonContract s, int i, string adjacentFile)
    {
        try
        {
            string? filePath = null;
            if (s.File is not null)
            {
                filePath = Path.Combine(Path.GetDirectoryName(adjacentFile)!, s.File);
            }
            string testPath = Path.GetRelativePath(_root.FullName, filePath ?? adjacentFile);
            var handler = _handlers[s.Type];
            var paramType = handler.GetType().GetInterfaces().Single(t => t.Name == "ITestHandler`1").GetGenericArguments()[0];
            var pparams = (ITestParameters)Activator.CreateInstance(typeof(TestParameters<>).MakeGenericType(paramType))!;
            pparams.Setup(s, i, testPath, filePath, adjacentFile, _loader);

            return (ITest)Activator.CreateInstance(typeof(Test<>).MakeGenericType(paramType), handler, pparams)!;
        }
        catch (Exception ex)
        {
            return new FailedTest(ex, s.Name ?? s.File, i);
        }
    }
}

public class FileLoader
{
    private readonly Dictionary<string, byte[]> _loaded = new Dictionary<string, byte[]>();

    internal byte[]? Load(string v)
    {
        if (!_loaded.ContainsKey(v))
        {
            _loaded[v] = File.ReadAllBytes(v);
        }

        return _loaded[v];
    }
}

public class TsJsonContract
{
    public required string Type { get; set; }
    public string? File { get; set; }
    public string? Name { get; set; }
    public required JsonElement Parameters { get; set; }
}

public class Test<T> : ITest
{
    private readonly ITestHandler<T> _handler;
    private readonly TestParameters<T> _pparams;

    public Test(ITestHandler<T> handler, TestParameters<T> pparams)
    {
        _handler = handler;
        _pparams = pparams;
    }

    public string Name => _pparams.TestName;

    public Task Run(TestResult result)
    {
        return _handler.RunTest(_pparams, result);
    }
}

public interface ITest
{
    string Name { get; }

    Task Run(TestResult result);
}

internal class FailedTest : ITest
{
    private readonly Exception _exception;
    private readonly string? _file;
    private readonly int _i;

    public string Name => $"{_file}:{_i}";

    public FailedTest(Exception exception, string? file, int i)
    {
        _exception = exception;
        _file = file;
        _i = i;
    }

    public Task Run(TestResult result)
    {
        result.AddResult("failed_run", false, _exception.ToString());
        return Task.CompletedTask;
    }
}
