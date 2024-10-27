using System.Text.Json;
using System.Text.Json.Serialization;
using TestManager.PluginLib;

namespace TestManager.Testing;

internal interface ILoadedTest : ITest
{
    static ILoadedTest Create(Type parameterType, ISecretLoader secretLoader, TestLoader.FileLoader fileLoader)
    {
        var testType = typeof(Test<>).MakeGenericType(parameterType);
        var test = Activator.CreateInstance(testType, secretLoader, fileLoader) as ILoadedTest;

        return test ?? throw new Exception("Issue creating test");
    }

    void SetParameters(JsonElement parameters);
    void SetHandler(ITestHandler handler);
}

internal class Test<T> : ILoadedTest
{
    private ITestHandler<T>? _handler;
    private ITestParameters<T>? _parameters;
    private readonly ISecretLoader _secretLoader;
    private readonly TestLoader.FileLoader _fileLoader;

    public Dictionary<string, string>? IntegrationParameters { get; set; }
    public string? FilePath { get; set; }
    public string? TestName { get; set; }
    public string? TestFilePath { get; set; }

    public Test(ISecretLoader secretLoader, TestLoader.FileLoader fileLoader)
    {
        _secretLoader = secretLoader;
        _fileLoader = fileLoader;
    }

    public async Task<TestResult> Run()
    {
        var result = new TestResult(this);
        if (_handler is null || _parameters is null)
        {
            throw new Exception("Tried to run test with missing params and/or test");
        }

        await _handler.RunTest(_parameters, result);
        return result;
    }

    public void SetHandler(ITestHandler handler)
    {
        if (handler is ITestHandler<T> castedHandler)
        {
            _handler = castedHandler;
        }
        else
        {
            throw new Exception("Somehow tried to assigned wrong handler");
        }
    }

    private static readonly JsonSerializerOptions _options = new JsonSerializerOptions() { NumberHandling = JsonNumberHandling.AllowReadingFromString };
    public void SetParameters(JsonElement parameters)
    {
        _parameters = new TestParameters<T>(_fileLoader)
        {
            Parameters = JsonSerializer.Deserialize<T>(parameters, _options)!,
            SecretLoader = _secretLoader,
            TestName = TestName!,
            FileName = FilePath!
        };
    }
}
