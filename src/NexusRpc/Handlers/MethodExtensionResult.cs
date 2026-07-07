namespace NexusRpc.Handlers
{
    /// <summary>
    /// Result of an <see cref="IMethodExtension"/> matching a method: the operation name it
    /// handles and the operation handler for it.
    /// </summary>
    public sealed class MethodExtensionResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MethodExtensionResult"/> class.
        /// </summary>
        /// <param name="operationName">Nexus operation name this handler is for. Must match one of
        /// the operations on the service definition.</param>
        /// <param name="handler">Operation handler in generic form. Extensions that start from a
        /// strongly-typed <see cref="IOperationHandler{TInput, TResult}"/> should call
        /// <see cref="OperationHandler.WrapAsGenericHandler{TInput, TResult}(IOperationHandler{TInput, TResult})"/>
        /// to obtain this.</param>
        public MethodExtensionResult(
            string operationName, IOperationHandler<object?, object?> handler)
        {
            OperationName = operationName;
            Handler = handler;
        }

        /// <summary>
        /// Gets the operation name this handler is for.
        /// </summary>
        public string OperationName { get; }

        /// <summary>
        /// Gets the operation handler in generic form.
        /// </summary>
        public IOperationHandler<object?, object?> Handler { get; }
    }
}
