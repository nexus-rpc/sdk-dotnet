using System;

namespace NexusRpc
{
    /// <summary>
    /// Attribute to put on an interface representing the contract of a Nexus service. Interface
    /// methods will have the <see cref="NexusOperationAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, Inherited = false)]
    public sealed class NexusServiceAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NexusServiceAttribute"/> class with the
        /// default name. See <see cref="Name" />.
        /// </summary>
        public NexusServiceAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NexusServiceAttribute"/> class with the
        /// given name.
        /// </summary>
        /// <param name="name">Nexus service name to use. See <see cref="Name" />.</param>
        public NexusServiceAttribute(string name) => Name = name;

        /// <summary>
        /// Gets the Nexus service name. If this is unset, it defaults to the unqualified type name
        /// of this interface. If the first character is a capital "I" followed by another capital
        /// letter, the "I" is trimmed when creating the default name.
        /// </summary>
        public string? Name { get; }
    }
}
