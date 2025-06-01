using GOL.Domain.Entities;

namespace GOL.Domain.Interfaces
{
    /// <summary>
    /// Service for processing board state iterations.
    /// </summary>
    public interface IBoardStateProcessingService
    {
        /// <summary>
        /// Processes board iterations and returns the final state.
        /// </summary>
        /// <param name="boardId">The board identifier to process</param>
        /// <param name="iterations">Number of iterations to process</param>
        /// <param name="shortCircuitFinalState">Whether to stop when reaching a final state</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The final board state after processing</returns>
        Task<BoardState> ProcessBoardIterationsAsync(
            Guid boardId, 
            long iterations, 
            bool shortCircuitFinalState, 
            CancellationToken cancellationToken);
    }
}