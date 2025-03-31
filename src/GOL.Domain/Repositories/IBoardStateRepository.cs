using GOL.Domain.Entities;

namespace GOL.Domain.Repositories
{
    /// <summary>
    /// Repository interface for Board entities.
    /// </summary>
    public interface IBoardStateRepository
    {
        /// <summary>
        /// Adds a new board state.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        Task AddAsync(BoardState board);

        /// <summary>
        /// Get lastest board state by boardId.
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        Task<BoardState?> GetLatestByBoardIdAsync(Guid boardId);

        /// <summary>
        /// Get lastest found board state by boardId and hash.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        Task<BoardState?> GetLastestByBoardIdAndHashAsync(Guid boardId, ulong hash);

        /// <summary>
        /// Save changes.
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync();
    }
}
