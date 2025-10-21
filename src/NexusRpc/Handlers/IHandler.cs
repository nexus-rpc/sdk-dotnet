using System.Threading.Tasks;

namespace NexusRpc.Handlers
{
    /// <summary>
    /// Interface for handling all inbound operations regardless of service. The primary
    /// implementation is at <see cref="Handler"/>.
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Start operation.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="input">Operation input.</param>
        /// <returns>Task with sync or async result.</returns>
        /// <exception cref="OperationException">Operation failed.</exception>
        /// <exception cref="HandlerException">Unexpected handler failure.</exception>
        Task<OperationStartResult<HandlerContent>> StartOperationAsync(
            OperationStartContext context,
            HandlerContent input);

        /// <summary>
        /// Request operation cancel.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Task when cancel has been sent.</returns>
        /// <exception cref="HandlerException">Unexpected handler failure.</exception>
        Task CancelOperationAsync(OperationCancelContext context);
    }
}