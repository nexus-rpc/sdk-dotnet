#pragma warning disable SA1402 // We allow multiple types of the same name

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Helper for results of an operation start.
    /// </summary>
    public static class OperationStartResult
    {
        /// <summary>
        /// Create sync result with fixed result value.
        /// </summary>
        /// <typeparam name="TResult">Result type.</typeparam>
        /// <param name="resultValue">Synchronous result value.</param>
        /// <returns>Start result with the result value.</returns>
        public static OperationStartResult<TResult> SyncResult<TResult>(TResult resultValue) =>
            new(resultValue, null);

        /// <summary>
        /// Create an async result with a token.
        /// </summary>
        /// <typeparam name="TResult">Result type, unused.</typeparam>
        /// <param name="asyncOperationToken">Token.</param>
        /// <returns>Start result with an async token.</returns>
        public static OperationStartResult<TResult> AsyncResult<TResult>(string asyncOperationToken) =>
            new(default, asyncOperationToken);
    }

    /// <summary>
    /// Representation of a start result.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    public class OperationStartResult<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStartResult{T}"/> class.
        /// </summary>
        /// <param name="syncResultValue">Sync result value if any.</param>
        /// <param name="asyncOperationToken">Async result token if any.</param>
        internal OperationStartResult(
            T? syncResultValue,
            string? asyncOperationToken)
        {
            SyncResultValue = syncResultValue;
            AsyncOperationToken = asyncOperationToken;
        }

        /// <summary>
        /// Gets the sync result value. May be null if set as null _or_ this result is async. Use
        /// <see cref="IsSync"/> to determine whether result is sync or async, not this value.
        /// </summary>
        public T? SyncResultValue { get; init; }

        /// <summary>
        /// Gets the async token if async, or null if sync.
        /// </summary>
        public string? AsyncOperationToken { get; init; }

        /// <summary>
        /// Gets a value indicating whether the result is sync.
        /// </summary>
        public bool IsSync => AsyncOperationToken is null;
    }
}