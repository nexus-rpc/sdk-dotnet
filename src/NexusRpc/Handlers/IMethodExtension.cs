using System.Reflection;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Extension point used by <c>ServiceHandlerInstance.FromInstance</c> to allow additional
    /// attributes on service-handler methods to produce operation handlers.
    /// </summary>
    /// <remarks>
    /// <para>Extensions are invoked for every method on the service handler class that does not
    /// have a <see cref="NexusOperationHandlerAttribute"/> and that maps by method name to an
    /// operation on the service. Extensions are given the raw <see cref="MethodInfo"/> plus the
    /// matched <see cref="OperationDefinition"/> and return the operation handler when they
    /// recognize the method, or <c>null</c> otherwise.</para>
    /// <para>If two extensions (or an extension and <see cref="NexusOperationHandlerAttribute"/>)
    /// register a handler for the same operation, <c>ServiceHandlerInstance.FromInstance</c>
    /// throws.</para>
    /// </remarks>
    public interface IMethodExtension
    {
        /// <summary>
        /// Inspect the given method and, if it is recognized, produce an operation handler for it.
        /// </summary>
        /// <param name="instance">Service handler instance.</param>
        /// <param name="method">Method being inspected.</param>
        /// <param name="operationDefinition">Operation the method maps to, matched by method name.
        /// </param>
        /// <returns>The operation handler in generic form if the method is recognized, or
        /// <c>null</c> to defer to the next extension. Extensions that start from a strongly-typed
        /// <see cref="IOperationHandler{TInput, TResult}"/> should call
        /// <see cref="OperationHandler.WrapAsGenericHandler{TInput, TResult}(IOperationHandler{TInput, TResult})"/>
        /// to obtain this.</returns>
        IOperationHandler<object?, object?>? Extract(
            object instance, MethodInfo method, OperationDefinition operationDefinition);
    }
}
