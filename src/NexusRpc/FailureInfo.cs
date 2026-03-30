using System.Collections.Generic;

namespace NexusRpc
{
    /// <summary>
    /// Represents a protocol-level failure with metadata and details.
    /// </summary>
    public sealed class FailureInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FailureInfo"/> class.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="metadata">Optional key-value metadata.</param>
        /// <param name="details">Optional arbitrary details.</param>
        /// <param name="stackTrace">Optional stack trace string.</param>
        /// <param name="cause">Optional cause of this failure.</param>
        public FailureInfo(
            string message,
            IReadOnlyDictionary<string, string>? metadata = null,
            string? details = null,
            string? stackTrace = null,
            FailureInfo? cause = null)
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
        public string? Details { get; private init; }

        /// <summary>
        /// Gets the optional stack trace string.
        /// </summary>
        public string? StackTrace { get; private init; }

        /// <summary>
        /// Gets the optional cause of this failure.
        /// </summary>
        public FailureInfo? Cause { get; private init; }
    }
}
