using System;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Mark a method as an "operation handler factory" for an operation on a class with a
    /// <see cref="NexusServiceHandlerAttribute"/> attribute. A method with this attribute must
    /// exist for every operation defined on a <see cref="NexusServiceAttribute"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class NexusOperationHandlerAttribute : Attribute
    {
    }
}
