using System.Reflection;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Extension point used by <c>ServiceHandlerInstance.FromInstance</c> to allow additional
    /// attributes on service-handler methods to produce operation handlers.
    /// </summary>
    /// <remarks>
    /// <para>Extensions are invoked for every method on the service handler class that does not
    /// have a <see cref="NexusOperationHandlerAttribute"/>. Extensions are given the raw
    /// <see cref="MethodInfo"/> plus the resolved <see cref="ServiceDefinition"/> and return an
    /// <see cref="Handlers.MethodExtensionResult"/> when they recognize the method, or
    /// <c>null</c> otherwise.</para>
    /// <para>If two extensions (or an extension and <see cref="NexusOperationHandlerAttribute"/>)
    /// register a handler for the same operation name, <c>ServiceHandlerInstance.FromInstance</c>
    /// throws.</para>
    /// </remarks>
    public interface IMethodExtension
    {
        /// <summary>
        /// Inspect the given method and, if it is recognized, produce an operation handler for it.
        /// </summary>
        /// <param name="instance">Service handler instance.</param>
        /// <param name="method">Method being inspected.</param>
        /// <param name="serviceDefinition">Resolved service definition for the handler class.
        /// </param>
        /// <returns>A <see cref="Handlers.MethodExtensionResult"/> if the method is recognized, or
        /// <c>null</c> to defer to the next extension.</returns>
        MethodExtensionResult? Extract(
            object instance, MethodInfo method, ServiceDefinition serviceDefinition);
    }

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
