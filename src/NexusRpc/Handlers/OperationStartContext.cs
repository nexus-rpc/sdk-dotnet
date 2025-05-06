#pragma warning disable CA1068 // Don't need cancellation token last here

using System;
using System.Collections.Generic;
using System.Threading;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Context provided when start is called.
    /// </summary>
    /// <param name="Service">Service name.</param>
    /// <param name="Operation">Operation name.</param>
    /// <param name="CancellationToken">Cancellation token for when this specific call is
    /// canceled (not to be confused with operation cancellation).</param>
    /// <param name="RequestId">Unique identifier for this start call for deduplication.</param>
    public record OperationStartContext(
        string Service,
        string Operation,
        CancellationToken CancellationToken,
        string RequestId) : OperationContext(Service, Operation, CancellationToken)
    {
        /// <summary>
        /// Gets the ptional callback for asynchronous operations to deliver results to.
        /// </summary>
        /// <remarks>
        /// If this is present and the implementation is an asynchronous operation, the
        /// implementation should ensure this callback is invoked with the result upon completion.
        /// </remarks>
#pragma warning disable CA1056 // We want a string even though this is a "URI-like" property.
        public string? CallbackUrl { get; init; }
#pragma warning restore CA1056

        // TODO(cretz): Document that this dictionary compares keys case-insensitively

        /// <summary>
        /// Gets headers to use on the callback if <see cref="CallbackUrl"/> is used.
        /// </summary>
        /// <remarks>
        /// This dictionary is expected to compare keys case-insensitively.
        /// </remarks>
        public IReadOnlyDictionary<string, string>? CallbackHeaders { get; init; }

        /// <summary>
        /// Gets links with arbitrary caller information.
        /// </summary>
        public IReadOnlyCollection<NexusLink> InboundLinks { get; init; } = Array.Empty<NexusLink>();
    }
}