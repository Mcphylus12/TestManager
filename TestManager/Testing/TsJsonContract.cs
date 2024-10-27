using System.Text.Json;

namespace TestManager.Testing;

public class TsJsonContract
{
    public required string Type { get; set; }
    public string? Name { get; set; }
    public required JsonElement Parameters { get; set; }
    public Dictionary<string, string>? IntegrationParameter { get; set; }
}
