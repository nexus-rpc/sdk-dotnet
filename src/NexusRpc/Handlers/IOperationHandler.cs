using System.Threading.Tasks;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Handler for an operation.
    /// </summary>
    /// <typeparam name="TInput">Operation input type. This should be <see cref="NoValue"/> to
    /// represent no parameter.</typeparam>
    /// <typeparam name="TResult">Operation result type. This should be <see cref="NoValue"/> to
    /// represent void return type.</typeparam>
    public interface IOperationHandler<TInput, TResult>
    {
        /// <summary>
        /// Support for accessing the underlying handler for when handlers are wrapped.
        /// </summary>
        public interface IWrapper
        {
            /// <summary>
            /// Gets the immediate underlying handler.
            /// </summary>
            /// <remarks>
            /// This is often only useful in middleware that needs to access a handler instance.
            /// Users may need to continually invoke this if they need further wrapped handlers.
            /// </remarks>
            IOperationHandler<TInput, TResult> Underlying { get; }
        }

        /// <summary>
        /// Start operation.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="input">Input.</param>
        /// <returns>Task with sync or async result.</returns>
        /// <exception cref="OperationException">Operation failed.</exception>
        /// <exception cref="HandlerException">Unexpected handler failure.</exception>
        Task<OperationStartResult<TResult>> StartAsync(
            OperationStartContext context,
            TInput input);

        /// <summary>
        /// Request operation cancel.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Task when cancel has been sent.</returns>
        /// <exception cref="HandlerException">Unexpected handler failure.</exception>
        Task CancelAsync(OperationCancelContext context);
    }
}