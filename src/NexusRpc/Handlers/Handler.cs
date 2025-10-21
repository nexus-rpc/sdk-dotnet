using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Primary <see cref="Handler"/> implementation that works with a collection of
    /// <see cref="ServiceHandlerInstance"/>s, a serializer, and middleware.
    /// </summary>
    public class Handler : IHandler
    {
        private readonly Dictionary<string, ServiceHandlerInstance> instances;
        private readonly ISerializer serializer;
        private readonly List<IOperationMiddleware> middlewaresInReverse;

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="instances">Service instances. Cannot have duplicates for the same service
        /// name.</param>
        /// <param name="serializer">Serializer to use.</param>
        public Handler(
            ICollection<ServiceHandlerInstance> instances,
            ISerializer serializer)
            : this(instances, serializer, Array.Empty<IOperationMiddleware>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Handler"/> class.
        /// </summary>
        /// <param name="instances">Service instances. Cannot have duplicates for the same service
        /// name.</param>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="middlewares">Middleware to apply.</param>
        public Handler(
            ICollection<ServiceHandlerInstance> instances,
            ISerializer serializer,
            IReadOnlyCollection<IOperationMiddleware> middlewares)
        {
            if (instances.Count == 0)
            {
                throw new ArgumentException("Must have at least one instance");
            }
            this.instances = new Dictionary<string, ServiceHandlerInstance>(instances.Count);
            foreach (var instance in instances)
            {
                if (this.instances.ContainsKey(instance.Definition.Name))
                {
                    throw new ArgumentException($"Duplicate Nexus service named {instance.Definition.Name}");
                }
                this.instances[instance.Definition.Name] = instance;
            }
            this.serializer = serializer;
            middlewaresInReverse = new(middlewares);
            middlewaresInReverse.Reverse();
        }

        /// <inheritdoc/>
        public async Task<OperationStartResult<HandlerContent>> StartOperationAsync(
            OperationStartContext context, HandlerContent input)
        {
            // Get handler
            var instance = GetInstance(context);
            var handler = GetInterceptedHandler(context, instance);
            var opDef = instance.Definition.Operations[context.Operation];

            // Need input type to be NoValue if it's void like
            var inputType = NoValue.IsVoidType(opDef.InputType) ? typeof(NoValue) : opDef.InputType;

            // Deserialize input if not void, letting any exception throw unwrapped
            var inputObject = await serializer.DeserializeAsync(
                new(input.ConsumeAllBytes(), input.Headers),
                inputType).ConfigureAwait(false);

            // Invoke handler
            var result = await handler.StartAsync(context, inputObject).ConfigureAwait(false);

            // If async, just return, if sync then serialize
            if (result.AsyncOperationToken is { } token)
            {
                return OperationStartResult.AsyncResult<HandlerContent>(token);
            }

            // Change sync result value to NoValue if return type is void
            var syncResult = result.SyncResultValue;
            if (opDef.OutputType == typeof(void))
            {
                syncResult = default(NoValue);
            }

            // Let any serialize exception throw unwrapped
            var resultContent = await serializer.SerializeAsync(syncResult).ConfigureAwait(false);
            return OperationStartResult.SyncResult(
                new HandlerContent(resultContent.Data, resultContent.Headers));
        }

        /// <inheritdoc/>
        public Task CancelOperationAsync(OperationCancelContext context)
        {
            // Get handler
            var instance = GetInstance(context);
            var handler = GetInterceptedHandler(context, instance);

            // Cancel
            return handler.CancelAsync(context);
        }

        private ServiceHandlerInstance GetInstance(OperationContext context)
        {
            if (!instances.TryGetValue(context.Service, out var instance))
            {
                throw new HandlerException(
                    HandlerErrorType.NotFound,
                    $"Unrecognized service {context.Service} or operation {context.Operation}");
            }
            return instance;
        }

        private IOperationHandler<object?, object?> GetInterceptedHandler(
            OperationContext context,
            ServiceHandlerInstance instance)
        {
            if (!instance.OperationHandlers.TryGetValue(context.Operation, out var handler))
            {
                throw new HandlerException(
                    HandlerErrorType.NotFound,
                    $"Unrecognized service {context.Service} or operation {context.Operation}");
            }
            foreach (var middleware in middlewaresInReverse)
            {
                handler = middleware.Intercept(context, handler);
            }
            return handler;
        }
    }
}