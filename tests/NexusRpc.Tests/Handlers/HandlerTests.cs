#pragma warning disable CA1822 // Don't require members to be static

namespace NexusRpc.Tests.Handlers;

using System.Text;
using System.Threading.Tasks;
using NexusRpc.Handlers;
using Xunit;
using Xunit.Abstractions;

public class HandlerTests : TestBase
{
    public HandlerTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [NexusService]
    public interface ISimpleService
    {
        [NexusOperation]
        string SayHello(string name);
    }

    [NexusServiceHandler(typeof(ISimpleService))]
    public class SimpleSyncService
    {
        [NexusOperationHandler]
        public IOperationHandler<string, string> SayHello() =>
            OperationHandler.Sync<string, string>((context, name) => $"Hello, {name}");
    }

    [Fact]
    public async Task Handler_SyncResult_Works()
    {
        var handler = new Handler(
            [ServiceHandlerInstance.FromInstance(new SimpleSyncService())],
            new NexusJsonSerializer());

        // Start is basic result
        var result = await handler.StartOperationAsync(
            new(
                Service: "SimpleService",
                Operation: "SayHello",
                CancellationToken: default,
                RequestId: Guid.NewGuid().ToString()),
            new(Encoding.UTF8.GetBytes("\"some-name\"")));
        Assert.Null(result.AsyncOperationToken);
        Assert.Equal(
            "\"Hello, some-name\"",
            Encoding.UTF8.GetString(result.SyncResultValue!.ConsumeAllBytes()));

        // Ensure other operation calls fail
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            handler.FetchOperationResultAsync(new(
                Service: "SimpleService",
                Operation: "SayHello",
                CancellationToken: default,
                OperationToken: Guid.NewGuid().ToString())));
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            handler.FetchOperationInfoAsync(new(
                Service: "SimpleService",
                Operation: "SayHello",
                CancellationToken: default,
                OperationToken: Guid.NewGuid().ToString())));
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            handler.CancelOperationAsync(new(
                Service: "SimpleService",
                Operation: "SayHello",
                CancellationToken: default,
                OperationToken: Guid.NewGuid().ToString())));
    }

    [NexusServiceHandler(typeof(ISimpleService))]
    public class SimpleAsyncService
    {
        public ICollection<OperationContext> Calls { get; } = new List<OperationContext>();

        [NexusOperationHandler]
        public IOperationHandler<string, string> SayHello() => new AsyncOperationHandler(Calls);

        public class AsyncOperationHandler(ICollection<OperationContext> Calls) : IOperationHandler<string, string>
        {
            public async Task<OperationStartResult<string>> StartAsync(
                OperationStartContext context, string input)
            {
                Calls.Add(context);
                return OperationStartResult.AsyncResult<string>($"{input}-token");
            }

            public async Task<string> FetchResultAsync(OperationFetchResultContext context)
            {
                Calls.Add(context);
                var name = context.OperationToken.Substring(0, context.OperationToken.Length - "-token".Length);
                return $"Hello, {name}";
            }

            public async Task<OperationInfo> FetchInfoAsync(OperationFetchInfoContext context)
            {
                Calls.Add(context);
                return new(context.OperationToken, OperationState.Running);
            }

            public async Task CancelAsync(OperationCancelContext context) =>
                Calls.Add(context);
        }
    }

    public class TrackingMiddleware : IOperationMiddleware
    {
        public ICollection<OperationContext> Calls { get; } = new List<OperationContext>();

        public IOperationHandler<object?, object?> Intercept(
            OperationContext context, IOperationHandler<object?, object?> nextHandler) =>
            new TrackingHandler(nextHandler, Calls);

        private class TrackingHandler(
            IOperationHandler<object?, object?> Next,
            ICollection<OperationContext> Calls) : IOperationHandler<object?, object?>
        {
            public Task<OperationStartResult<object?>> StartAsync(
                OperationStartContext context, object? input)
            {
                Calls.Add(context);
                return Next.StartAsync(context, input);
            }

            public Task<object?> FetchResultAsync(OperationFetchResultContext context)
            {
                Calls.Add(context);
                return Next.FetchResultAsync(context);
            }

            public Task<OperationInfo> FetchInfoAsync(OperationFetchInfoContext context)
            {
                Calls.Add(context);
                return Next.FetchInfoAsync(context);
            }

            public Task CancelAsync(OperationCancelContext context)
            {
                Calls.Add(context);
                return Next.CancelAsync(context);
            }
        }
    }

    [Fact]
    public async Task Handler_AsyncResultAndMiddleware_Works()
    {
        var service = new SimpleAsyncService();
        // We'll also check middleware while we're here
        var middleware = new TrackingMiddleware();
        var handler = new Handler(
            [ServiceHandlerInstance.FromInstance(service)],
            new NexusJsonSerializer(),
            [middleware]);

        // Make the 4 calls
        var startResult = await handler.StartOperationAsync(
            new(
                Service: "SimpleService",
                Operation: "SayHello",
                CancellationToken: default,
                RequestId: Guid.NewGuid().ToString()),
            new(Encoding.UTF8.GetBytes("\"some-name\"")));
        Assert.Null(startResult.SyncResultValue);
        Assert.NotNull(startResult.AsyncOperationToken);
        var result = await handler.FetchOperationResultAsync(new(
            Service: "SimpleService",
            Operation: "SayHello",
            CancellationToken: default,
            OperationToken: startResult.AsyncOperationToken));
        Assert.Equal(
            "\"Hello, some-name\"",
            Encoding.UTF8.GetString(result.ConsumeAllBytes()));
        var infoResult = await handler.FetchOperationInfoAsync(new(
            Service: "SimpleService",
            Operation: "SayHello",
            CancellationToken: default,
            OperationToken: startResult.AsyncOperationToken));
        Assert.Equal(startResult.AsyncOperationToken, infoResult.Token);
        Assert.Equal(OperationState.Running, infoResult.State);
        await handler.CancelOperationAsync(new(
            Service: "SimpleService",
            Operation: "SayHello",
            CancellationToken: default,
            OperationToken: startResult.AsyncOperationToken));

        // Confirm contexts
        void AssertCalls(ICollection<OperationContext> calls)
        {
            Assert.Equal(4, calls.Count);
            Assert.Equal(
                [
                    typeof(OperationStartContext), typeof(OperationFetchResultContext),
                    typeof(OperationFetchInfoContext), typeof(OperationCancelContext),
                ],
                service.Calls.Select(c => c.GetType()).ToList());
            Assert.All(calls, call => Assert.Equal("SimpleService", call.Service));
            Assert.All(calls, call => Assert.Equal("SayHello", call.Operation));
        }
        AssertCalls(service.Calls);
        AssertCalls(middleware.Calls);
    }

    [Fact]
    public async Task Handler_ManuallyCreated_Works()
    {
        var opHandler = new SimpleAsyncService.AsyncOperationHandler(new List<OperationContext>());
        var instance = new ServiceHandlerInstance(
            new ServiceDefinition(
                "ManualService",
                [new("ManualSayHello", typeof(string), typeof(string))]),
            new Dictionary<string, IOperationHandler<object?, object?>>
            {
                ["ManualSayHello"] = OperationHandler.WrapAsGenericHandler(opHandler),
            });
        var handler = new Handler([instance], new NexusJsonSerializer());

        // Start and check result
        var startResult = await handler.StartOperationAsync(
            new(
                Service: "ManualService",
                Operation: "ManualSayHello",
                CancellationToken: default,
                RequestId: Guid.NewGuid().ToString()),
            new(Encoding.UTF8.GetBytes("\"some-name\"")));
        Assert.NotNull(startResult.AsyncOperationToken);
        var result = await handler.FetchOperationResultAsync(new(
            Service: "ManualService",
            Operation: "ManualSayHello",
            CancellationToken: default,
            OperationToken: startResult.AsyncOperationToken));
        Assert.Equal(
            "\"Hello, some-name\"",
            Encoding.UTF8.GetString(result.ConsumeAllBytes()));
    }

    [Fact]
    public async Task Handler_MissingHandler_Throws()
    {
        var handler = new Handler(
            [ServiceHandlerInstance.FromInstance(new SimpleSyncService())],
            new NexusJsonSerializer());

        // Unknown service
        var exc = await Assert.ThrowsAsync<HandlerException>(() =>
            handler.StartOperationAsync(
                new(
                    Service: "NotFoundService",
                    Operation: "SayHello",
                    CancellationToken: default,
                    RequestId: Guid.NewGuid().ToString()),
                new(Encoding.UTF8.GetBytes("\"some-name\""))));
        Assert.Equal("Unrecognized service NotFoundService or operation SayHello", exc.Message);
        Assert.Equal(HandlerErrorType.NotFound, exc.ErrorType);

        // Unknown operation
        exc = await Assert.ThrowsAsync<HandlerException>(() =>
            handler.StartOperationAsync(
                new(
                    Service: "SayHelloService",
                    Operation: "NotFoundSayHello",
                    CancellationToken: default,
                    RequestId: Guid.NewGuid().ToString()),
                new(Encoding.UTF8.GetBytes("\"some-name\""))));
        Assert.Equal("Unrecognized service SayHelloService or operation NotFoundSayHello", exc.Message);
        Assert.Equal(HandlerErrorType.NotFound, exc.ErrorType);
    }

    [NexusService]
    public interface IVoidService
    {
        [NexusOperation]
        void NoReturnNoParam();
    }

    [NexusServiceHandler(typeof(IVoidService))]
    public class VoidService
    {
        [NexusOperationHandler]
        public IOperationHandler<NoValue, NoValue> NoReturnNoParam() => new VoidOperationHandler();

        private class VoidOperationHandler : IOperationHandler<NoValue, NoValue>
        {
            public async Task<OperationStartResult<NoValue>> StartAsync(
                OperationStartContext context, NoValue input) =>
                OperationStartResult.SyncResult(default(NoValue));

            public async Task<NoValue> FetchResultAsync(
                OperationFetchResultContext context) => default;

            public Task<OperationInfo> FetchInfoAsync(
                OperationFetchInfoContext context) => throw new NotImplementedException();

            public Task CancelAsync(
                OperationCancelContext context) => throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task Handler_VoidService_Works()
    {
        var serializer = new NexusJsonSerializer();
        var handler = new Handler(
            [ServiceHandlerInstance.FromInstance(new VoidService())],
            serializer);

        var startResult = await handler.StartOperationAsync(
             new(
                Service: "VoidService",
                Operation: "NoReturnNoParam",
                CancellationToken: default,
                RequestId: Guid.NewGuid().ToString()),
             new(Array.Empty<byte>()));
        Assert.Empty(startResult.SyncResultValue!.ConsumeAllBytes());

        var result = await handler.FetchOperationResultAsync(
             new(
                Service: "VoidService",
                Operation: "NoReturnNoParam",
                CancellationToken: default,
                OperationToken: Guid.NewGuid().ToString()));
        Assert.Empty(result.ConsumeAllBytes());

        // Check that serialize was called with the start and fetch results
        Assert.Equal([default(NoValue), default(NoValue)], serializer.SerializeCalls.ToArray());

        // Check that deserialize was called with the start param
        var (content, type) = serializer.DeserializeCalls.Single();
        Assert.Empty(content.Data);
        Assert.Equal(typeof(NoValue), type);
    }
}