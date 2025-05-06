using System;

namespace NexusRpc
{
    /// <summary>
    /// Attribute put on interface methods of a <see cref="NexusServiceAttribute"/> interface. If
    /// the interface method is inherited/overridden, all must have the same matching attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class NexusOperationAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NexusOperationAttribute"/> class with the
        /// default name. See <see cref="Name" />.
        /// </summary>
        public NexusOperationAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NexusOperationAttribute"/> class with the
        /// given name.
        /// </summary>
        /// <param name="name">Nexus operation name to use. See <see cref="Name" />.</param>
        public NexusOperationAttribute(string name) => Name = name;

        /// <summary>
        /// Gets the Nexus operation name. If this is unset, it defaults to the unqualified method
        /// name.
        /// </summary>
        public string? Name { get; }
    }
}
