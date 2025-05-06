using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NexusRpc
{
    /// <summary>
    /// Definition of a Nexus service. Most users do not instantiate this directly, it is implicitly
    /// instantiated by the system. For those that do, <see cref="FromType"/> is the common way to
    /// obtain a definition from an interface.
    /// </summary>
    /// <param name="Name">Service name.</param>
    /// <param name="Operations">Collection of operation definitions, keyed by operation name.
    /// </param>
    public record ServiceDefinition(
        string Name,
        IReadOnlyDictionary<string, OperationDefinition> Operations)
    {
        private static readonly ConcurrentDictionary<Type, ServiceDefinition> Definitions = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceDefinition"/> class. This is a
        /// helper to convert a collection of operations into a dictionary.
        /// </summary>
        /// <param name="name">Service name.</param>
        /// <param name="operations">Collection of operation definitions.</param>
        public ServiceDefinition(string name, IEnumerable<OperationDefinition> operations)
            : this(name, operations.ToDictionary(o => o.Name, o => o))
        {
        }

        /// <summary>
        /// Create a service definition from an interface type. This is memoized by type to make
        /// successive calls quick.
        /// </summary>
        /// <typeparam name="T">Interface type.</typeparam>
        /// <returns>Service definition for the type.</returns>
        public static ServiceDefinition FromType<T>() => FromType(typeof(T));

        /// <summary>
        /// Create a service definition from an interface type. This is memoized by type to make
        /// successive calls quick.
        /// </summary>
        /// <param name="type">Interface type.</param>
        /// <returns>Service definition for the type.</returns>
        public static ServiceDefinition FromType(Type type) =>
            Definitions.GetOrAdd(type, CreateFromType);

        private static ServiceDefinition CreateFromType(Type type)
        {
            if (!type.IsInterface)
            {
                throw new ArgumentException("Must be an interface");
            }
            var serviceAttr = type.GetCustomAttribute<NexusServiceAttribute>() ??
                throw new ArgumentException("Missing NexusService attribute");

            // Use name from attr or if not present, the unqualified interface name
            var name = serviceAttr.Name ?? type.Name;
            // Trim off leading "I" if not manually set and two-capitalized-char start. This does
            // mean that IPAddress becomes PAddress, but we expect .NET developers to follow the "I"
            // prefix approach for interfaces or they can set the name explicitly if needed.
            if (serviceAttr.Name == null && name.Length > 1 && name[0] == 'I' && char.IsUpper(name[1]))
            {
                name = name.Substring(1);
            }

            // Collect all methods with operation attributes
            var operations = new Dictionary<string, OperationDefinition>();
            var errs = new List<string>();
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute<NexusOperationAttribute>() == null)
                {
                    continue;
                }
                try
                {
                    var opDef = OperationDefinition.FromMethod(method);
                    // If it's already there, it has to match
                    if (!operations.TryGetValue(opDef.Name, out var existing))
                    {
                        operations[opDef.Name] = opDef;
                    }
                    else if (existing != opDef)
                    {
                        errs.Add(
                            $"Operation definition on {method.Name} on {method.DeclaringType?.Name} " +
                            "mismatches against another operation of the same name/signature in the hierarchy");
                        continue;
                    }
                }
#pragma warning disable CA1031 // Ok catching general exception type here
                catch (Exception e)
#pragma warning restore CA1031
                {
                    errs.Add(
                        $"Operation definition on {method.Name} on {method.DeclaringType?.Name} is invalid: " +
                            e.Message);
                }
            }
            // If there are any errors, throw
            if (errs.Count > 0)
            {
                throw new AggregateException(errs.Select(err => new ArgumentException(err)));
            }
            // Need at least one operation
            if (operations.Count == 0)
            {
                throw new ArgumentException("No operations found on service");
            }
            return new(name, operations);
        }
    }
}