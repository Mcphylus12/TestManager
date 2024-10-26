namespace TestManager.PluginLib;

public class TestException : Exception
{
    public TestException()
    {
    }

    public TestException(string? message) : base(message)
    {
    }
}
