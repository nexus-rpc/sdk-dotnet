using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexusRpc
{
    /// <summary>
    /// Serializer for Nexus bytes to/from .NET values.
    /// </summary>
    public interface ISerializer
    {
        /// <summary>
        /// Serialize the given value as content. This is always called even for absent type/value
        /// (absent parameter or void return type).
        /// </summary>
        /// <param name="value">Value to convert. The value will be <see cref="Handlers.NoValue"/> to
        /// represent absence of a type/value (i.e. absent
        /// parameter or void return type).</param>
        /// <returns>Task with raw content.</returns>
        Task<Content> SerializeAsync(object? value);

        /// <summary>
        /// Deserialize the given content into an object of the given type or null. This is always
        /// called even for absent type/value (absent parameter or void return type).
        /// </summary>
        /// <param name="content">Content to convert.</param>
        /// <param name="type">Type to deserialize into. This is the <see cref="Handlers.NoValue"/>
        /// type to represent absence of a type/value (i.e. absent parameter or void return type).
        /// </param>
        /// <returns>Task with created object of the given type.</returns>
        Task<object?> DeserializeAsync(Content content, Type type);

        /// <summary>
        /// Raw byte content for use by a serializer.
        /// </summary>
        /// <param name="Data">Byte array of data.</param>
        /// <param name="Headers">Optional headers.</param>
        public record Content(
#pragma warning disable CA1819 // We're ok with a byte array in this case
            byte[] Data,
#pragma warning restore CA1819
            IReadOnlyDictionary<string, string>? Headers = null);
    }
}