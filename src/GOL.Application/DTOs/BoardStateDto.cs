using GOL.Domain.Entities;

namespace GOL.Application.DTOs
{
    /// <summary>
    /// Represents a board with their current state.
    /// </summary>
    public class BoardStateDto
    {
        /// <summary>
        /// Board identifier.
        /// </summary>
        public Guid BoardId { get; set; }

        /// <summary>
        /// Living cells.
        /// </summary>
        public List<CellCoordinates> LiveCells { get; set; }

        /// <summary>
        /// Current state iteration.
        /// </summary>
        public long Iteration { get; set; }

        /// <summary>
        /// Current status of the board.
        /// </summary>
        public string Status { get; set; }
    }
}
