namespace Toolkit.FileTester;

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
