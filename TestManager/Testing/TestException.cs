namespace Toolkit.FileTester;

public class TestException : Exception
{
    public TestException()
    {
    }

    public TestException(string? message) : base(message)
    {
    }
}
