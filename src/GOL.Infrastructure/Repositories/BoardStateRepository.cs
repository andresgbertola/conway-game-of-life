using GOL.Domain.Entities;
using GOL.Domain.Repositories;
using GOL.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GOL.Infrastructure.Repositories
{
    /// <summary>
    /// Represents BoardStateRepository.
    /// </summary>
    public class BoardStateRepository : IBoardStateRepository
    {
        private readonly GOLDbContext _dbContext;

        public BoardStateRepository(GOLDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Adds a new board state.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>
        public async Task AddAsync(BoardState board)
        {
            await _dbContext.BoardStates.AddAsync(board);
        }

        /// <summary>
        /// Get lastest board state by boardId.
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        public async Task<BoardState?> GetLatestByBoardIdAsync(Guid boardId)
        {
            return await _dbContext.BoardStates.Where(x => x.BoardId == boardId).OrderByDescending(x => x.Iteration).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get lastest found board state by boardId and hash.
        /// </summary>
        /// <param name="boardId"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<BoardState?> GetLastestByBoardIdAndHashAsync(Guid boardId, ulong hash)
        {
            return await _dbContext.BoardStates.Where(x => x.BoardId == boardId && x.StateHash == hash).OrderByDescending(x => x.Iteration).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Save changes.
        /// </summary>
        /// <returns></returns>
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
