using System;

namespace NexusRpc
{
    /// <summary>
    /// Operation failed or was canceled.
    /// </summary>
    public class OperationException : Exception
    {
        private OperationException(
            OperationState state,
            string message,
            Exception? innerException)
            : base(message, innerException) => State = state;

        /// <summary>
        /// Gets the operation state.
        /// </summary>
        public OperationState State { get; private init; }

        /// <summary>
        /// Create an operation exception representing an operation failure.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="innerException">Inner exception if any.</param>
        /// <returns>Operation exception.</returns>
        public static OperationException CreateFailure(string message, Exception? innerException = null) =>
            new(OperationState.Failed, message, innerException);

        /// <summary>
        /// Create an operation exception representing an canceled operation.
        /// </summary>
        /// <param name="message">Failure message.</param>
        /// <param name="innerException">Inner exception if any.</param>
        /// <returns>Operation exception.</returns>
        public static OperationException CreateCanceled(string message, Exception? innerException = null) =>
            new(OperationState.Canceled, message, innerException);
    }
}