namespace NexusRpc.Handlers
{
    /// <summary>
    /// Error type of a <see cref="HandlerException"/>.
    /// </summary>
    public enum HandlerErrorType
    {
        /// <summary>
        /// The error type is unknown. Subsequent requests by the client are permissible.
        /// </summary>
        Unknown,

        /// <summary>
        /// The server cannot or will not process the request due to an apparent client error.
        /// Clients should not retry this request unless advised otherwise.
        /// </summary>
        BadRequest,

        /// <summary>
        /// The client did not supply valid authentication credentials for this request. Clients
        /// should not retry this request unless advised otherwise.
        /// </summary>
        Unauthenticated,

        /// <summary>
        /// The caller does not have permission to execute the specified operation. Clients should
        /// not retry this request unless advised otherwise.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// The requested resource could not be found but may be available in the future. Clients
        /// should not retry this request unless advised otherwise.
        /// </summary>
        NotFound,

        /// <summary>
        /// Some resource has been exhausted, perhaps a per-user quota, or perhaps the entire file
        /// system is out of space. Subsequent requests by the client are permissible.
        /// </summary>
        ResourceExhausted,

        /// <summary>
        /// An internal error occurred. Subsequent requests by the client are permissible.
        /// </summary>
        Internal,

        /// <summary>
        /// The server either does not recognize the request method, or it lacks the ability to
        /// fulfill the request. Clients should not retry this request unless advised otherwise.
        /// </summary>
        NotImplemented,

        /// <summary>
        /// The service is currently unavailable. Subsequent requests by the client are permissible.
        /// </summary>
        Unavailable,

        /// <summary>
        /// Used by gateways to report that a request to an upstream server has timed out.
        /// Subsequent requests by the client are permissible.
        /// </summary>
        UpstreamTimeout,
    }
}