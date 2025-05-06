using System;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Mark a class as a service handler. This requires a service type be provided that is an
    /// interface with a <see cref="NexusServiceAttribute"/> attribute. For each
    /// <see cref="NexusOperationAttribute"/> method defined in the given service type, this class
    /// must contain the same-named method with a <see cref="NexusOperationHandlerAttribute"/>
    /// attribute and that returns an operation handler to handle that operation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NexusServiceHandlerAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NexusServiceHandlerAttribute"/> class with
        /// the given service type.
        /// </summary>
        /// <param name="serviceType">Nexus service interface type this implements. Must be an
        /// interface with the <see cref="NexusServiceAttribute"/> attribute.</param>
        public NexusServiceHandlerAttribute(Type serviceType) => ServiceType = serviceType;

        /// <summary>
        /// Gets the Nexus service interface type.
        /// </summary>
        public Type ServiceType { get; }
    }
}
