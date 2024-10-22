
using System.Text.Json;
using TestManager.Management;
using TestManager.Testing;

namespace TestManager;

internal class FormGenerator
{
    private Dictionary<string, ITestHandler> _handlers;

    public FormGenerator(Dictionary<string, ITestHandler> handlers)
    {
        _handlers = handlers;
    }

    internal TestForm Generate(List<TsJsonContract> testData)
    {
        var result = new TestForm()
        {
            AvailableHandlers = _handlers.Select(kv => GetForm(kv.Key, kv.Value)).ToList(),
            CurrentHandlers = testData.Select(td => new HandlerForm
            {
                Name = td.Type!,
                Parameters = td.Parameters.EnumerateObject().Select(o => new ConfigInput()
                {
                    Name = o.Name,
                    Value = o.Value.GetString(),
                    Type = GetType(o.Value.ValueKind)
                }).ToList()
            }).ToList()
        };

        return result;
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
                Parameters = JsonSerializer.SerializeToElement(item.Parameters.ToDictionary(ci => ci.Name, ci => ci.Value))
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
