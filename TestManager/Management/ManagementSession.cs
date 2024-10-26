using System.Text.Json;
using Microsoft.Extensions.Logging;
using TestManager.FileTraversal;
using TestManager.PluginLib;
using TestManager.Testing;

namespace TestManager.Management;

public class ManagementSession
{
    private readonly DirectoryInfo _root;
    private readonly ITestResultIntegrator? _integrator;
    private readonly FileFinder _fileFinder;
    private readonly JsonSerializerOptions _options;
    private readonly TestLoader _loader;
    private readonly FormGenerator _formGenerator;
    private readonly TestRunner _runner;

    public ManagementSession(DirectoryInfo root, Dictionary<string, ITestHandler> handlers, ITestResultIntegrator? integrator, ISecretLoader secretLoader)
    {
        _root = root;
        _integrator = integrator;
        _fileFinder = new FileFinder(root);
        _options = new JsonSerializerOptions { WriteIndented = true };
        _loader = new TestLoader(handlers, root, secretLoader);
        _formGenerator = new FormGenerator(handlers);
        _runner = new TestRunner();
    }

    public IEnumerable<Entry> GetFiles(string mode, string? path = null)
    {
        if (mode == "file")
        {
            return _fileFinder.GetDirectoryContents(path);
        }
        else if (mode == "bulk")
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new NotSupportedException("need to supploy a path for bulk");
            }

            return _fileFinder.GetMatchingTestFiles(path, true).Select(fi => new Entry(fi.Name.EndsWith(".tsjson") ? "testfile" : "file", fi.Name, Path.GetRelativePath(_root.FullName, fi.Directory.FullName).Replace('\\', '/')));
        }
        throw new NotSupportedException("unsupported mode");
    }

    public async Task<TestForm> Load(string file)
    {
        var path = Path.Combine(_root.FullName, file);
        var testData = await TestLoader.LoadTestFile(new FileInfo(path));
        return _formGenerator.Generate(testData, _integrator?.FieldDefinitions);
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

        await (_integrator?.SubmitResults(result.TestResults) ?? Task.CompletedTask);

        return result.ToJson()!;
    }

    internal async Task<string> BulkRun(string pattern)
    {
        var files = _fileFinder.GetMatchingTestFiles(pattern);
        var tests = await _loader.LoadTests(files);
        var result = await _runner.RunTests(tests);
        return result.ToJson()!;
    }

    internal void DeleteTestFile(string file)
    {
        if (file.Contains("..") || !file.EndsWith(".tsjson"))
        {
            throw new Exception("Invalid file to delete: " + file);
        }

        File.Delete(Path.Combine(_root.FullName, file));
    }

    internal bool CreateTestFile(Entry newFile)
    {
        if (newFile.Path.Contains("..") || newFile.Name.Contains(".."))
        {
            throw new Exception("Invalid file to create: " + newFile.Path);
        }

        var targetNewFile = new FileInfo(Path.Combine(_root.FullName, newFile.Path, newFile.Name + ".tsjson"));

        if (targetNewFile.Exists)
        {
            return false;
        }

        File.WriteAllText(targetNewFile.FullName, "[]");
        return true;
    }
}

public class TestForm
{
    public required List<HandlerForm> AvailableHandlers { get; set; }

    public required List<HandlerForm> CurrentHandlers { get; set; }
    public ISet<string>? IntegrationParameters { get; set; }
}

public class HandlerForm
{
    public required string Name { get; set; }
    public required List<ConfigInput> Parameters { get; set; }
    public string? TestName { get; set; }
    public Dictionary<string, string>? TestIntegrationParameters { get; set; }
}

public class ConfigInput
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public string? Value { get; set; }
}
