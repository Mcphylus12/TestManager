using System.Text.Json;
using TestManager.FileTraversal;
using TestManager.Testing;

namespace TestManager.Management;

public class ManagementSession
{
    private readonly DirectoryInfo _root;
    private readonly FileFinder _fileFinder;
    private readonly JsonSerializerOptions _options;
    private readonly TestLoader _loader;
    private readonly FormGenerator _formGenerator;
    private readonly TestRunner _runner;

    public ManagementSession(DirectoryInfo root, Dictionary<string, ITestHandler> handlers)
    {
        _root = root;
        _fileFinder = new FileFinder(root);
        _options = new JsonSerializerOptions { WriteIndented = true };
        _loader = new TestLoader(handlers, root);
        _formGenerator = new FormGenerator(handlers);
        _runner = new TestRunner();
    }

    public IEnumerable<Entry> GetFiles(string? path = null)
    {
        return _fileFinder.GetTestFilesAndFolders(path);
    }

    public async Task<TestForm> Load(string file)
    {
        var path = Path.Combine(_root.FullName, file);
        var testData = await TestLoader.LoadTestFile(new FileInfo(path));
        return _formGenerator.Generate(testData);
    }

    public async Task Save(string file, HandlerForm[] newData)
    {
        var path = Path.Combine(_root.FullName, file);
        var contracts = _formGenerator.ParseForm(newData);
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(contracts, _options));
    }

    public async Task<string> Run(string file)
    {
        var tests = await _loader.LoadTests([new FileInfo(Path.Combine(_root.FullName, file))]);
        var result = await _runner.RunTests(tests);
        return result.ToJson()!;
    }
}

public class TestForm
{
    public required List<HandlerForm> AvailableHandlers { get; set; }

    public required List<HandlerForm> CurrentHandlers { get; set; }
}

public class HandlerForm
{
    public required string Name { get; set; }
    public required List<ConfigInput> Parameters { get; set; }
}

public class ConfigInput
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Value { get; set; }
}
