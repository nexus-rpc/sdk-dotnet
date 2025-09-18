using System.Collections.Generic;
using System.Threading;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Base class for contexts provided on different operation calls.
    /// </summary>
    /// <param name="Service">Service name.</param>
    /// <param name="Operation">Operation name.</param>
    /// <param name="CancellationToken">Cancellation token for when this specific call is
    /// canceled (not to be confused with operation cancellation).</param>
    public abstract record OperationContext(
        string Service,
        string Operation,
        CancellationToken CancellationToken)
    {
        /// <summary>
        /// Gets outbound links to set on response. This is intentionally mutable as handlers and
        /// middleware are welcome to add links.
        /// </summary>
        public IList<NexusLink> OutboundLinks { get; init; } = new List<NexusLink>();

        // Document that there is no lock that is making this be set atomically at the same time as
        // cancel is invoked. Creator of context will set this to a non-null value just before the
        // token is canceled. Also document this is cancellation reason for the token, not related
        // to Nexus operation cancellation.

        /// <summary>
        /// Gets or sets the cancellation reason if cancellation token is canceled.
        /// </summary>
        /// <remarks>
        /// This is the reason the specific operation call and cancellation token are canceled and
        /// is not related to actual operation cancellation.
        /// </remarks>
        /// <remarks>
        /// There is no lock making sure this is atomically set at the same time cancellation token
        /// is canceled since that can perform arbitrary blocking. The creator of the context is
        /// expected to set this to a non-null value just before the token is canceled.
        /// </remarks>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Gets the set of headers for this call.
        /// </summary>
        /// <remarks>
        /// This dictionary is expected to compare keys case-insensitively.
        /// </remarks>
        public IReadOnlyDictionary<string, string>? Headers { get; init; }
    }
}