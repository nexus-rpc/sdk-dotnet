using System;
using NexusRpc;

internal static class Program
{
    private static void Main()
    {
        var definition = ServiceDefinition.FromType<ISmokeService>();
        var operation = definition.Operations["Echo"];
        if (definition.Name != "SmokeService" ||
            operation.InputType != typeof(string) ||
            operation.OutputType != typeof(string))
        {
            throw new InvalidOperationException("Packed NexusRpc service definition is invalid.");
        }
        Console.WriteLine("NexusRpc package smoke test passed.");
    }

    [NexusService]
    public interface ISmokeService
    {
        [NexusOperation]
        string Echo(string value);
    }
}
