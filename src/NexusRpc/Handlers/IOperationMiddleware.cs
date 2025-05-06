namespace NexusRpc.Handlers
{
    /// <summary>
    /// Middleware for intercepting operations.
    /// </summary>
    public interface IOperationMiddleware
    {
        /// <summary>
        /// Intercepts an operation. Called for each call on an operation.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="nextHandler">Next handler most middleware should delegate to.</param>
        /// <returns>An operation handler that will be used to make the call.</returns>
        IOperationHandler<object?, object?> Intercept(
            OperationContext context, IOperationHandler<object?, object?> nextHandler);
    }
}