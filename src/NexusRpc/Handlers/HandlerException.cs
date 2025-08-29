using System;
using System.Collections.Generic;
using System.Linq;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Thrown from a handler for any unexpected error.
    /// </summary>
    public class HandlerException : Exception
    {
        private static readonly Dictionary<string, HandlerErrorType> StringToErrorType = new()
        {
            ["UNKNOWN"] = HandlerErrorType.Unknown,
            ["BAD_REQUEST"] = HandlerErrorType.BadRequest,
            ["UNAUTHENTICATED"] = HandlerErrorType.Unauthenticated,
            ["UNAUTHORIZED"] = HandlerErrorType.Unauthorized,
            ["NOT_FOUND"] = HandlerErrorType.NotFound,
            ["RESOURCE_EXHAUSTED"] = HandlerErrorType.ResourceExhausted,
            ["INTERNAL"] = HandlerErrorType.Internal,
            ["NOT_IMPLEMENTED"] = HandlerErrorType.NotImplemented,
            ["UNAVAILABLE"] = HandlerErrorType.Unavailable,
            ["UPSTREAM_TIMEOUT"] = HandlerErrorType.UpstreamTimeout,
        };

        private static readonly Dictionary<HandlerErrorType, string> ErrorTypeToString =
            StringToErrorType.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private static readonly HashSet<HandlerErrorType> NonRetryableErrorTypes = new()
            {
                HandlerErrorType.BadRequest,
                HandlerErrorType.Unauthenticated,
                HandlerErrorType.Unauthorized,
                HandlerErrorType.NotFound,
                HandlerErrorType.NotImplemented,
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerException"/> class.
        /// </summary>
        /// <param name="errorType">Error type.</param>
        /// <param name="message">Message.</param>
        /// <param name="errorRetryBehavior">Error retry behavior.</param>
        public HandlerException(
            HandlerErrorType errorType,
            string message,
            HandlerErrorRetryBehavior errorRetryBehavior = HandlerErrorRetryBehavior.Unspecified)
            : this(errorType, message, null, errorRetryBehavior)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerException"/> class.
        /// </summary>
        /// <param name="errorType">Error type.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// <param name="errorRetryBehavior">Error retry behavior.</param>
        public HandlerException(
            HandlerErrorType errorType,
            string message,
            Exception? innerException,
            HandlerErrorRetryBehavior errorRetryBehavior = HandlerErrorRetryBehavior.Unspecified)
            : base(message, innerException)
        {
            RawErrorType = ErrorTypeToString.TryGetValue(errorType, out var v) ? v : "UNKNOWN";
            ErrorType = errorType;
            ErrorRetryBehavior = errorRetryBehavior;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlerException"/> class.
        /// </summary>
        /// <remarks>
        /// This only exists for deserialization in case error types changes. Users should not use
        /// this constructor directly.
        /// </remarks>
        /// <param name="rawErrorType">Error type string.</param>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        /// <param name="errorRetryBehavior">Error retry behavior.</param>
        public HandlerException(
            string rawErrorType,
            string message,
            Exception? innerException,
            HandlerErrorRetryBehavior errorRetryBehavior = HandlerErrorRetryBehavior.Unspecified)
            : base(message, innerException)
        {
            RawErrorType = rawErrorType;
            ErrorType = StringToErrorType.TryGetValue(rawErrorType, out var v) ? v : HandlerErrorType.Unknown;
            ErrorRetryBehavior = errorRetryBehavior;
        }

        /// <summary>
        /// Gets the raw error type as a string. Most users should use <see cref="ErrorType"/>.
        /// </summary>
        public string RawErrorType { get; private init; }

        /// <summary>
        /// Gets the error type.
        /// </summary>
        public HandlerErrorType ErrorType { get; private init; }

        /// <summary>
        /// Gets the error retry behavior.
        /// </summary>
        public HandlerErrorRetryBehavior ErrorRetryBehavior { get; private init; }

        /// <summary>
        /// Gets a value indicating whether the error should be retried.
        /// </summary>
        public bool IsRetryable =>
            ErrorRetryBehavior != HandlerErrorRetryBehavior.Unspecified ?
                ErrorRetryBehavior == HandlerErrorRetryBehavior.Retryable :
                    !NonRetryableErrorTypes.Contains(ErrorType);

        /// <summary>
        /// Advanced helper to parse error type string into an error type.
        /// </summary>
        /// <param name="errorTypeStr">Raw error type string.</param>
        /// <param name="errorType">Error type to set.</param>
        /// <returns>Whether it could be parsed.</returns>
        public static bool TryParseErrorType(string errorTypeStr, out HandlerErrorType errorType) =>
            StringToErrorType.TryGetValue(errorTypeStr, out errorType);
    }
}