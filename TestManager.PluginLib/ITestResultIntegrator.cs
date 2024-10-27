using System.Runtime.InteropServices;

namespace TestManager.PluginLib;

public interface ITestResultIntegrator
{
    static ITestResultIntegrator Create(Type concreteType, Dictionary<string, string>? config, ISecretLoader secretLoader)
    {
        if (!concreteType.IsAssignableTo(typeof(ITestResultIntegrator)))
        {
            throw new NotSupportedException("tried to create integrator with bad concrete type");
        }

        var result = concreteType.InvokeMember(nameof(Create), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.InvokeMethod, null, null, [config, secretLoader]);

        if (result is ITestResultIntegrator correctType)
        {
            return correctType;
        }

        throw new Exception("Issue creating integrator");
    }
    abstract static ITestResultIntegrator Create(Dictionary<string, string>? config, ISecretLoader secretLoader);
    ISet<string> FieldDefinitions { get; }
    Task SubmitResults(RunResult results);
}
