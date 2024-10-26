
using System.Text.Json;
using TestManager.Management;
using TestManager.PluginLib;
using TestManager.Testing;

namespace TestManager;

internal class FormGenerator
{
    private Dictionary<string, ITestHandler> _handlers;

    public FormGenerator(Dictionary<string, ITestHandler> handlers)
    {
        _handlers = handlers;
    }

    internal TestForm Generate(List<TsJsonContract> testData, ISet<string>? integrationParameters)
    {
        var result = new TestForm()
        {
            IntegrationParameters = integrationParameters,
            AvailableHandlers = _handlers.Select(kv => GetForm(kv.Key, kv.Value)).ToList(),
            CurrentHandlers = testData.Select(td => new HandlerForm
            {
                Name = td.Type!,
                TestName = td.Name,
                TestIntegrationParameters = PrepareIntegrationParameters(td.IntegrationParameter, integrationParameters),
                Parameters = td.Parameters.EnumerateObject().Select(o => new ConfigInput()
                {
                    Name = o.Name,
                    Value = o.Value.GetString(),
                    Type = GetType(o.Value.ValueKind)
                }).ToList(),
            }).ToList()
        };

        return result;
    }

    private static Dictionary<string, string>? PrepareIntegrationParameters(Dictionary<string, string>? existingIntegrationParameters, ISet<string>? availableIntegrationParameters)
    {
        if (availableIntegrationParameters is null)
        {
            return existingIntegrationParameters;
        }

        existingIntegrationParameters ??= new Dictionary<string, string>();

        foreach (var item in availableIntegrationParameters)
        {
            if (!existingIntegrationParameters.ContainsKey(item))
            {
                existingIntegrationParameters[item] = string.Empty;
            }
        }

        return existingIntegrationParameters;
    }

    private string GetType(JsonValueKind valueKind)
    {
        return valueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
            JsonValueKind.True or JsonValueKind.False => "bool",
            _ => "string"
        };
    }

    internal List<TsJsonContract> ParseForm(HandlerForm[] newData)
    {
        var result = new List<TsJsonContract>();

        foreach (var item in newData)
        {
            result.Add(new TsJsonContract
            {
                Type = item.Name,
                Name = string.IsNullOrWhiteSpace(item.TestName) ? null : item.TestName,
                Parameters = JsonSerializer.SerializeToElement(item.Parameters.ToDictionary(ci => ci.Name, ci => ci.Value)),
                IntegrationParameter = item.TestIntegrationParameters
            });
        }

        return result;
    }

    private static HandlerForm GetForm(string key, ITestHandler testHandler)
    {
        return new HandlerForm
        {
            Name = key,
            Parameters = testHandler.GetType().GetInterface("ITestHandler`1")!.GetGenericArguments().Single().GetProperties().Select(p => new ConfigInput
            {
                Name = p.Name,
                Type = GetType(p.PropertyType),
            }).ToList()
        };
    }

    private static string GetType(Type propertyType)
    {
        if (propertyType == typeof(string)) return "string";
        if (propertyType == typeof(long)) return "number";
        if (propertyType == typeof(int)) return "number";
        if (propertyType == typeof(double)) return "number";
        if (propertyType == typeof(float)) return "number";
        throw new NotSupportedException();
    }
}
