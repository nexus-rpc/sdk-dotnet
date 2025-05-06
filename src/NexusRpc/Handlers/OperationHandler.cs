using System;
using System.Linq;
using System.Threading.Tasks;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Helpers for <see cref="IOperationHandler{TInput, TResult}"/>s.
    /// </summary>
    public static class OperationHandler
    {
        /// <summary>
        /// Create a synchronous (start-only) operation from the given async lambda.
        /// </summary>
        /// <typeparam name="TInput">Operation input type.</typeparam>
        /// <typeparam name="TResult">Operation result type.</typeparam>
        /// <param name="apply">Lambda that is invoked on each operation start.</param>
        /// <returns>Operation handler that only supports start with synchronous response.</returns>
        public static IOperationHandler<TInput, TResult> Sync<TInput, TResult>(
            Func<OperationStartContext, TInput, Task<TResult>> apply) =>
            new SynchronousOperationHandler<TInput, TResult>(apply);

        /// <summary>
        /// Create a synchronous (start-only) operation from the given lambda.
        /// </summary>
        /// <typeparam name="TInput">Operation input type.</typeparam>
        /// <typeparam name="TResult">Operation result type.</typeparam>
        /// <param name="apply">Lambda that is invoked on each operation start.</param>
        /// <returns>Operation handler that only supports start with synchronous response.</returns>
        public static IOperationHandler<TInput, TResult> Sync<TInput, TResult>(
            Func<OperationStartContext, TInput, TResult> apply) =>
            Sync<TInput, TResult>((ctx, inp) => Task.Run(() => apply(ctx, inp)));

        /// <summary>
        /// Wrap the given handler in a generic form.
        /// </summary>
        /// <typeparam name="TInput">Handler input type.</typeparam>
        /// <typeparam name="TResult">Handler output type.</typeparam>
        /// <param name="handler">Handler instance to wrap in a generic form.</param>
        /// <returns>A generic wrapper of the operation handler.</returns>
        public static IOperationHandler<object?, object?> WrapAsGenericHandler<TInput, TResult>(
            IOperationHandler<TInput, TResult> handler) =>
            WrapAsGenericHandler(handler, typeof(IOperationHandler<TInput, TResult>));

        /// <summary>
        /// Wrap the given handler in a generic form.
        /// </summary>
        /// <param name="handler">An instance of <see cref="IOperationHandler{TInput, TResult}"/> to
        /// wrap in a generic form.</param>
        /// <param name="interfaceType"><see cref="IOperationHandler{TInput, TResult}"/> type with
        /// its generic arguments.</param>
        /// <returns>A generic wrapper of the operation handler.</returns>
        public static IOperationHandler<object?, object?> WrapAsGenericHandler(
            object handler, Type interfaceType)
        {
            // Confirm it's an operation handler type
            if (!interfaceType.IsInterface ||
                !interfaceType.IsGenericType ||
                interfaceType.GetGenericTypeDefinition() != typeof(IOperationHandler<,>))
            {
                throw new ArgumentException($"Expected operation handler type, got {interfaceType}");
            }
            // Confirm the handler implements the type
            if (!handler.GetType().GetInterfaces().Contains(interfaceType))
            {
                throw new ArgumentException($"Handler of type {handler.GetType()} does not implement {interfaceType}");
            }
            // Generically create an instance of the wrapper
            var genericType = typeof(GenericOperationHandler<,>).MakeGenericType(
                interfaceType.GetGenericArguments());
            return (IOperationHandler<object?, object?>)Activator.CreateInstance(genericType, handler)!;
        }

        private sealed class SynchronousOperationHandler<TInput, TResult> : IOperationHandler<TInput, TResult>
        {
            private readonly Func<OperationStartContext, TInput, Task<TResult>> apply;

            public SynchronousOperationHandler(
                Func<OperationStartContext, TInput, Task<TResult>> apply) =>
                this.apply = apply;

            public async Task<OperationStartResult<TResult>> StartAsync(
                OperationStartContext context,
                TInput input)
            {
                var result = await apply(context, input).ConfigureAwait(false);
                return OperationStartResult.SyncResult(result);
            }

            public Task<TResult> FetchResultAsync(OperationFetchResultContext context) =>
                throw new NotImplementedException("Not supported on sync operation");

            public Task<OperationInfo> FetchInfoAsync(OperationFetchInfoContext context) =>
                throw new NotImplementedException("Not supported on sync operation");

            public Task CancelAsync(OperationCancelContext context) =>
                throw new NotImplementedException("Not supported on sync operation");
        }

#pragma warning disable CA1812 // This is instantiated reflectively
        private sealed class GenericOperationHandler<TInput, TResult> :
            IOperationHandler<object?, object?>, IOperationHandler<TInput, TResult>.IWrapper
        {
#pragma warning restore CA1812
            public GenericOperationHandler(IOperationHandler<TInput, TResult> underlying) =>
                Underlying = underlying;

            public IOperationHandler<TInput, TResult> Underlying { get; private init; }

            public async Task<OperationStartResult<object?>> StartAsync(
                OperationStartContext context,
                object? input)
            {
                if (input is not TInput typedInput)
                {
                    throw new ArgumentException($"Expected input type of {typeof(TInput)}, but got {input?.GetType()}");
                }
                var result = await Underlying.StartAsync(context, typedInput).ConfigureAwait(false);
                return new(result.SyncResultValue, result.AsyncOperationToken);
            }

            public async Task<object?> FetchResultAsync(OperationFetchResultContext context) =>
                await Underlying.FetchResultAsync(context).ConfigureAwait(false);

            public Task<OperationInfo> FetchInfoAsync(OperationFetchInfoContext context) =>
                Underlying.FetchInfoAsync(context);

            public Task CancelAsync(OperationCancelContext context) =>
                Underlying.CancelAsync(context);
        }
    }
}