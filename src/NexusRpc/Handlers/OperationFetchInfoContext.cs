#pragma warning disable CA1068 // Don't need cancellation token last here

using System.Threading;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Context provided when fetch info is called.
    /// </summary>
    /// <param name="Service">Service name.</param>
    /// <param name="Operation">Operation name.</param>
    /// <param name="CancellationToken">Cancellation token for when this specific call is
    /// canceled (not to be confused with operation cancellation).</param>
    /// <param name="OperationToken">Token referencing the operation.</param>
    public record OperationFetchInfoContext(
        string Service,
        string Operation,
        CancellationToken CancellationToken,
        string OperationToken) : OperationContext(Service, Operation, CancellationToken);
}