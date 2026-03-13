using System.Collections.Generic;

namespace NexusRpc
{
    /// <summary>
    /// Represents a protocol-level failure with metadata and details.
    /// </summary>
    public sealed class Failure
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Failure"/> class.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="metadata">Optional key-value metadata.</param>
        /// <param name="details">Optional arbitrary details.</param>
        /// <param name="stackTrace">Optional stack trace string.</param>
        /// <param name="cause">Optional cause of this failure.</param>
        public Failure(
            string message,
            IReadOnlyDictionary<string, string>? metadata = null,
            IReadOnlyDictionary<string, object>? details = null,
            string? stackTrace = null,
            Failure? cause = null)
        {
            Message = message;
            Metadata = metadata;
            Details = details;
            StackTrace = stackTrace;
            Cause = cause;
        }

        /// <summary>
        /// Gets the failure message.
        /// </summary>
        public string Message { get; private init; }

        /// <summary>
        /// Gets the optional key-value metadata.
        /// </summary>
        public IReadOnlyDictionary<string, string>? Metadata { get; private init; }

        /// <summary>
        /// Gets the optional arbitrary details.
        /// </summary>
        public IReadOnlyDictionary<string, object>? Details { get; private init; }

        /// <summary>
        /// Gets the optional stack trace string.
        /// </summary>
        public string? StackTrace { get; private init; }

        /// <summary>
        /// Gets the optional cause of this failure.
        /// </summary>
        public Failure? Cause { get; private init; }
    }
}
