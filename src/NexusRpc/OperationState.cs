namespace NexusRpc
{
    /// <summary>
    /// State an operation can be in.
    /// </summary>
    public enum OperationState
    {
        /// <summary>
        /// Indicates an operation is started and not yet completed.
        /// </summary>
        Running,

        /// <summary>
        /// Indicates an operation completed successfully.
        /// </summary>
        Succeeded,

        /// <summary>
        /// Indicates an operation completed as failed.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates an operation completed as canceled.
        /// </summary>
        Canceled,
    }
}