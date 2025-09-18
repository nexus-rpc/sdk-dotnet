#pragma warning disable CA1068 // Don't need cancellation token last here

using System;
using System.Threading;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Context provided when fetch result is called.
    /// </summary>
    /// <param name="Service">Service name.</param>
    /// <param name="Operation">Operation name.</param>
    /// <param name="CancellationToken">Cancellation token for when this specific call is
    /// canceled (not to be confused with operation cancellation).</param>
    /// <param name="OperationToken">Token referencing the operation.</param>
    public record OperationFetchResultContext(
        string Service,
        string Operation,
        CancellationToken CancellationToken,
        string OperationToken) : OperationContext(Service, Operation, CancellationToken)
    {
        /// <summary>
        /// Gets the timeout for how long the user wants to wait on the result.
        /// </summary>
        /// <remarks>
        /// If this value is null, the result or <see cref="OperationStillRunningException"/> should
        /// be returned/thrown right away. If this value is present, the fetch result call should
        /// try to wait up until this duration or until an implementer chosen maximum, whichever
        /// ends sooner, before returning the result or throwing
        /// <see cref="OperationStillRunningException"/>.
        /// </remarks>
        public TimeSpan? Timeout { get; init; }
    }
}