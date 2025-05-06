namespace NexusRpc.Handlers
{
    /// <summary>
    /// Retry behavior of a <see cref="HandlerException"/>.
    /// </summary>
    public enum HandlerErrorRetryBehavior
    {
        /// <summary>
        /// Retry behavior determined by the <see cref="HandlerException.ErrorType"/>.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Handler call should be retried, overriding <see cref="HandlerException.ErrorType"/>
        /// default behavior.
        /// </summary>
        Retryable,

        /// <summary>
        /// Handler call should not be retried, overriding <see cref="HandlerException.ErrorType"/>
        /// default behavior.
        /// </summary>
        NonRetryable,
    }
}