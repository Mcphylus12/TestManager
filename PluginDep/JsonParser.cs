
using System.Text.Json;

namespace PluginDep;

public static class JsonParser
{
    public static JsonDocument Parse(byte[]? file) => JsonDocument.Parse(file);
}
