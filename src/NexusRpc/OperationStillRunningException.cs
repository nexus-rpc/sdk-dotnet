using System;

namespace NexusRpc
{
    /// <summary>
    /// An operation result was requested, but it is still running.
    /// </summary>
    public class OperationStillRunningException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OperationStillRunningException"/> class.
        /// </summary>
        public OperationStillRunningException()
        {
        }
    }
}