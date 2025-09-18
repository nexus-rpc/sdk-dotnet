using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NexusRpc
{
    /// <summary>
    /// Definition of a Nexus service operation. Most users do not instantiate this directly, it is
    /// implicitly instantiated by the system. For those that do, <see cref="FromMethod"/> is the
    /// common way to obtain a definition from an interface method.
    /// </summary>
    /// <param name="Name">Operation name.</param>
    /// <param name="InputType">Input type. If there is no input type (i.e. no parameter), this is
    /// <c>typeof(void)</c>.</param>
    /// <param name="OutputType">Output type. If there is no output type (i.e. returns void), this
    /// is <c>typeof(void)</c>.</param>
    public record OperationDefinition(
        string Name,
        Type InputType,
        Type OutputType)
    {
        /// <summary>
        /// Gets the reflected interface method this definition is for if any.
        /// </summary>
        public MethodInfo? MethodInfo { get; init; }

        /// <summary>
        /// Create an operation definition from the given reflected interface method.
        /// </summary>
        /// <param name="method">Interface method.</param>
        /// <returns>Operation definition.</returns>
        public static OperationDefinition FromMethod(MethodInfo method)
        {
            var attr = method.GetCustomAttribute<NexusOperationAttribute>() ??
                throw new ArgumentException("Missing NexusOperation attribute");
            var parms = method.GetParameters();
            if (parms.Length > 1)
            {
                throw new ArgumentException("Can have no more than one parameter");
            }
            if (method.ContainsGenericParameters)
            {
                throw new ArgumentException("Cannot be generic");
            }
            if (method.IsStatic)
            {
                throw new ArgumentException("Cannot be static");
            }
            if (!method.IsAbstract)
            {
                throw new ArgumentException("Cannot have implementation");
            }
            if (typeof(Task).IsAssignableFrom(method.ReturnType))
            {
                throw new ArgumentException("Operation definitions should not be defined as tasks");
            }
            return new(
                attr.Name ?? method.Name,
                parms.FirstOrDefault()?.ParameterType ?? typeof(void),
                method.ReturnType)
            { MethodInfo = method };
        }
    }
}