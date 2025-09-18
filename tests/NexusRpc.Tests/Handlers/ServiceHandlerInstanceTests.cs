#pragma warning disable CA1822 // Don't require members to be static
#pragma warning disable CA1052 // Don't require classes to be static

namespace NexusRpc.Tests.Handlers;

using NexusRpc.Handlers;
using Xunit;
using Xunit.Abstractions;

public class ServiceHandlerInstanceTests : TestBase
{
    public ServiceHandlerInstanceTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [NexusService]
    public interface ISimpleService
    {
        [NexusOperation]
        string DoSomething(string param);
    }

    public class AttributeMissing
    {
    }

    [Fact]
    public void FromInstance_AttributeMissing_Bad() =>
        AssertBadInstance(new AttributeMissing(), "Missing NexusServiceHandler attribute");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class AttributeOnlyOnBase
    {
    }

    public class AttributeOnlyOnBaseSubclass : AttributeOnlyOnBase
    {
    }

    [Fact]
    public void FromInstance_AttributeOnlyOnBase_Bad() =>
        AssertBadInstance(new AttributeOnlyOnBaseSubclass(), "Missing NexusServiceHandler attribute");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationWithParams
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> DoSomething(string param) =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationWithParams_Bad() =>
        AssertBadInstance(new OperationWithParams(), "Cannot have parameters");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationWithGeneric
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> DoSomething<T>() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationWithGeneric_Bad() =>
        AssertBadInstance(new OperationWithGeneric(), "Cannot be generic");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationNotFound
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> DoSomethingUnknown() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationNotFound_Bad() =>
        AssertBadInstance(new OperationNotFound(), "No matching NexusOperation");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationBadReturn
    {
        [NexusOperationHandler]
        public string DoSomething() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationBadReturn_Bad() =>
        AssertBadInstance(new OperationBadReturn(), "Expected return type of");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationBadInputType
    {
        [NexusOperationHandler]
        public IOperationHandler<int, string> DoSomething() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationBadInputType_Bad() =>
        AssertBadInstance(new OperationBadInputType(), "Expected return type of");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationBadOutputType
    {
        [NexusOperationHandler]
        public IOperationHandler<string, int> DoSomething() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationBadOutputType_Bad() =>
        AssertBadInstance(new OperationBadOutputType(), "Expected return type of");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationFailToCreate
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> DoSomething() =>
            throw new NotImplementedException();
    }

    [Fact]
    public void FromInstance_OperationFailToCreate_Bad() =>
        Assert.IsType<NotImplementedException>(Assert.Throws<ArgumentException>(() =>
            ServiceHandlerInstance.FromInstance(new OperationFailToCreate())).InnerException);

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationReturnNull
    {
        [NexusOperationHandler]
#pragma warning disable CS8603 // Intentionally returning null here
        public IOperationHandler<string, string> DoSomething() => null;
#pragma warning restore CS8603
    }

    [Fact]
    public void FromInstance_OperationReturnNull_Bad() =>
        AssertBadInstance(new OperationReturnNull(), "handler was null");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationHandlerMissing
    {
    }

    [Fact]
    public void FromInstance_OperationHandlerMissing_Bad() =>
        AssertBadInstance(new OperationHandlerMissing(), "Missing handlers for defined operations: 'DoSomething'");

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationSimple
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> DoSomething() =>
            OperationHandler.Sync<string, string>((ctx, input) => "ignore");
    }

    [Fact]
    public void FromInstance_OperationSimple_Good() =>
        Assert.Equal(
            "DoSomething",
            Assert.Single(ServiceHandlerInstance.FromInstance(
                new OperationSimple()).OperationHandlers).Key);

    [NexusServiceHandler(typeof(ISimpleService))]
    public class OperationStatic
    {
        [NexusOperationHandler]
        public static IOperationHandler<string, string> DoSomething() =>
            OperationHandler.Sync<string, string>((ctx, input) => "ignore");
    }

    [Fact]
    public void FromInstance_OperationStatic_Good() =>
        Assert.Equal(
            "DoSomething",
            Assert.Single(ServiceHandlerInstance.FromInstance(
                new OperationStatic()).OperationHandlers).Key);

    private static void AssertBadInstance(object instance, params string[] anyErrorContains)
    {
        try
        {
            ServiceHandlerInstance.FromInstance(instance);
            Assert.Fail("Did not fail");
        }
        catch (ArgumentException e)
        {
            foreach (var v in anyErrorContains)
            {
                if (!e.Message.Contains(v) && e.InnerException?.Message?.Contains(v) != true)
                {
                    Assert.Fail($"'{v}' not in ${e}");
                }
            }
        }
    }
}