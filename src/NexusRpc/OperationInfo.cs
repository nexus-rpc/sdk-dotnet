namespace NexusRpc
{
    /// <summary>
    /// Information about an operation.
    /// </summary>
    /// <param name="Token">Operation token.</param>
    /// <param name="State">Operation state.</param>
    public record OperationInfo(string Token, OperationState State);
}