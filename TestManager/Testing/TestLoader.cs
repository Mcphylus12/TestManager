using System.Text.Json;
using TestManager.PluginLib;

namespace TestManager.Testing;
internal class TestLoader
{
    private readonly Dictionary<string, ITestHandler> _handlers;
    private readonly DirectoryInfo _root;
    private readonly ISecretLoader _secretLoader;
    private readonly FileLoader _fileLoader;

    public TestLoader(Dictionary<string, ITestHandler> handlers, DirectoryInfo root, ISecretLoader secretLoader)
    {
        _handlers = handlers;
        _root = root;
        _secretLoader = secretLoader;
        _fileLoader = new FileLoader();
    }

    public async Task<IEnumerable<ITest>> LoadTests(IEnumerable<FileInfo> testFiles)
    {
        var tests = new List<ITest>();
        foreach (var file in testFiles)
        {
            try
            {
                var testConfigurations = await LoadTestFile(file);

                foreach (var (testConfiguration, i) in testConfigurations.Select((testConfiguration, i) => (testConfiguration, i)))
                {
                    try
                    {
                        tests.Add(CreateTest(testConfiguration, i, file));
                    }
                    catch (TestLoadException ex)
                    {
                        tests.Add(ex.AsTest());
                    }
                }
            }
            catch (TestLoadException ex)
            {
                tests.Add(ex.AsTest());
            }
        }

        return tests;
    }

    public static async Task<List<TsJsonContract>> LoadTestFile(FileInfo file)
    {
        try
        {
            var fileContents = await File.ReadAllTextAsync(file.FullName);
            var testJson = JsonDocument.Parse(fileContents);
            var testConfigurations = LoadTests(testJson);
            return testConfigurations;
        }
        catch (Exception ex)
        {
            throw new TestLoadException(file, ex);
        }
    }

    private static List<TsJsonContract> LoadTests(JsonDocument testJson)
    {
        if (testJson.RootElement.ValueKind == JsonValueKind.Object)
        {
            return [testJson.Deserialize<TsJsonContract>()!];
        }
        else if (testJson.RootElement.ValueKind == JsonValueKind.Array)
        {
            return [.. testJson.Deserialize<IEnumerable<TsJsonContract>>()!];
        }
        else
        {
            throw new JsonException($"Test Contract root must be object or array. Found: {testJson.RootElement.ValueKind}");
        }
    }

    private ITest CreateTest(TsJsonContract testConfiguration, int testNumberInFile, FileInfo testFile)
    {
        try
        {
            var handler = _handlers[testConfiguration.Type];
            var testParameterType = handler.GetType().GetInterfaces().Single(t => t.Name == "ITestHandler`1").GetGenericArguments()[0];
            ILoadedTest test = ILoadedTest.Create(testParameterType, _secretLoader, _fileLoader);
            test.IntegrationParameters = testConfiguration.IntegrationParameter;
            test.TestFilePath = Path.GetRelativePath(_root.FullName, testFile.FullName);
            test.FilePath = testFile.GetFileUnderTest();
            test.TestName = !string.IsNullOrWhiteSpace(testConfiguration.Name) ?
                testConfiguration.Name :
                $"{Path.GetRelativePath(_root.FullName, testFile.FullName)}:{testNumberInFile}";

            test.SetParameters(testConfiguration.Parameters);
            test.SetHandler(handler);

            return test;
        }
        catch (Exception ex)
        {
            throw new TestLoadException(testFile, testConfiguration, testNumberInFile, ex);
        }
    }

    internal class FileLoader
    {
        private readonly Dictionary<string, byte[]?> _loaded = [];

        internal byte[]? Load(string v)
        {
            try
            {
                if (!_loaded.TryGetValue(v, out var value))
                {
                    value = File.ReadAllBytes(v);
                    _loaded[v] = value;
                }

                return value;
            }
            catch (FileNotFoundException)
            {
                _loaded[v] = null;
                return null;
            }
        }
    }
}

public static class FileInfoExtensions
{
    public static string GetFileUnderTest(this FileInfo testFile) => testFile.FullName[0..^7];
}
