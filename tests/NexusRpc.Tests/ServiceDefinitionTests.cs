namespace NexusRpc.Tests;

using Xunit;
using Xunit.Abstractions;

public class ServiceDefinitionTests : TestBase
{
    public ServiceDefinitionTests(ITestOutputHelper output)
        : base(output)
    {
    }

    public class NonInterface
    {
    }

    [Fact]
    public void FromType_NonInterface_Bad() =>
        AssertBadDefinitionType<NonInterface>("Must be an interface");

    public interface IAttributeMissing
    {
        void Whatever();
    }

    [Fact]
    public void FromType_AttributeMissing_Bad() =>
        AssertBadDefinitionType<IAttributeMissing>("Missing NexusService attribute");

    [NexusService]
    public interface INoOperations
    {
        void Whatever();
    }

    [Fact]
    public void FromType_NoOperations_Bad() =>
        AssertBadDefinitionType<INoOperations>("No operations found on service");

    [NexusService]
    public interface IBadService
    {
        [NexusOperation]
        string DuplicateOperation(string foo);

        [NexusOperation]
        int DuplicateOperation(int foo);

        [NexusOperation]
        string TwoParameters(string param1, string param2);

        [NexusOperation]
        string Generic<T>(string param);

        [NexusOperation]
        static string Static(string param) => "foo";

        [NexusOperation]
        string WithImplementation(string param) => "foo";

        [NexusOperation]
        Task<string> Async(string param);
    }

    [Fact]
    public void FromType_BadService_Bad() =>
        AssertBadDefinitionType<IBadService>(
            "another operation of the same name",
            "no more than one parameter",
            "Cannot be generic",
            "Cannot be static",
            "Cannot have implementation",
            "should not be defined as tasks");

    [NexusService]
    public interface IGoodService
    {
        [NexusOperation]
        string ParamAndResponse(string param);

        [NexusOperation]
        void ParamNoResponse(string param);

        [NexusOperation]
        string NoParamResponse();

        [NexusOperation("custom-name")]
        void CustomName();
    }

    [Fact]
    public void FromType_GoodService_Good()
    {
        var expected = new ServiceDefinition("GoodService", new Dictionary<string, OperationDefinition>
        {
            ["ParamAndResponse"] = new("ParamAndResponse", typeof(string), typeof(string)),
            ["ParamNoResponse"] = new("ParamNoResponse", typeof(string), typeof(void)),
            ["NoParamResponse"] = new("NoParamResponse", typeof(void), typeof(string)),
            ["custom-name"] = new("custom-name", typeof(void), typeof(void)),
        });
        var actual = ServiceDefinition.FromType<IGoodService>();
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(
            expected.Operations.ToDictionary(kv => kv.Key, kv => (kv.Value.Name, kv.Value.InputType, kv.Value.OutputType)),
            actual.Operations.ToDictionary(kv => kv.Key, kv => (kv.Value.Name, kv.Value.InputType, kv.Value.OutputType)));
    }

    private static void AssertBadDefinitionType<T>(params string[] anyErrorContains)
    {
        try
        {
            ServiceDefinition.FromType<T>();
            Assert.Fail("Did not fail");
        }
        catch (ArgumentException e)
        {
            foreach (var v in anyErrorContains)
            {
                Assert.Contains(v, e.Message);
            }
        }
        catch (AggregateException e)
        {
            foreach (var v in anyErrorContains)
            {
                var messages = e.InnerExceptions.Select(inner => inner.Message).ToList();
                if (!messages.Any(inner => inner.Contains(v)))
                {
                    Assert.Fail($"'{v}' not found in set of exceptions: '{string.Join("', '", messages)}'");
                }
            }
        }
    }
}