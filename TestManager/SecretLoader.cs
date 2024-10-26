using System.Text;
using TestManager.PluginLib;

namespace TestManager;
public class SecretLoader : ISecretLoader
{
    private readonly Dictionary<string, string> _secrets;

    public SecretLoader()
    {
        _secrets = new Dictionary<string, string>();
    }

    public string LoadSecret(string secretName)
    {
        if (_secrets.ContainsKey(secretName))
        {
            return _secrets[secretName];
        }

        var envValue = Environment.GetEnvironmentVariable("TestManager_" + secretName);

        if (envValue is not null)
        {
            _secrets[secretName] = envValue;
            return envValue;
        }

        Console.Write("Waiting for secret for " + secretName + ": ");
        var consoleSecret = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
                break;
            consoleSecret.Append(key.KeyChar);
        }
        Console.WriteLine();

        _secrets[secretName] = consoleSecret.ToString();
        return _secrets[secretName];
    }
}
