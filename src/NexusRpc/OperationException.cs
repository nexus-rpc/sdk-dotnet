using System;

namespace NexusRpc
{
    /// <summary>
    /// Operation failed or was canceled.
    /// </summary>
    public class OperationException : Exception
    {
        private readonly string? stackTraceOverride;

        private OperationException(
            OperationState state,
            string message,
            Exception? innerException,
            string? stackTrace = null,
            FailureInfo? originalFailure = null)
            : base(message, innerException)
        {
            State = state;
            stackTraceOverride = stackTrace;
            OriginalFailure = originalFailure;
        }

        /// <summary>
        /// Gets the operation state.
        /// </summary>
        public OperationState State { get; private init; }

        /// <summary>
        /// Gets the stack trace. If an explicit stack trace was provided, returns that;
        /// otherwise returns the runtime stack trace.
        /// </summary>
        public override string? StackTrace => stackTraceOverride ?? base.StackTrace;

        /// <summary>
        /// Gets the optional original failure information.
        /// </summary>
        public FailureInfo? OriginalFailure { get; private init; }

        /// <summary>
        /// Create an operation exception representing an operation failure.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="innerException">Inner exception if any.</param>
        /// <param name="stackTrace">Optional stack trace string.</param>
        /// <param name="originalFailure">Optional original failure information.</param>
        /// <returns>Operation exception.</returns>
        public static OperationException CreateFailed(
            string message,
            Exception? innerException = null,
            string? stackTrace = null,
            FailureInfo? originalFailure = null) =>
            new(OperationState.Failed, message, innerException, stackTrace, originalFailure);

        /// <summary>
        /// Create an operation exception representing an canceled operation.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="innerException">Inner exception if any.</param>
        /// <param name="stackTrace">Optional stack trace string.</param>
        /// <param name="originalFailure">Optional original failure information.</param>
        /// <returns>Operation exception.</returns>
        public static OperationException CreateCanceled(
            string message,
            Exception? innerException = null,
            string? stackTrace = null,
            FailureInfo? originalFailure = null) =>
            new(OperationState.Canceled, message, innerException, stackTrace, originalFailure);
    }
}