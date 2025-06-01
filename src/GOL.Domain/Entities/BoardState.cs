using GOL.Domain.Enums;
using GOL.Domain.Serializers;

namespace GOL.Domain.Entities
{
    /// <summary>
    /// Represents a Board.
    /// </summary>
    public class BoardState
    {
        /// <summary>
        /// Id.
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Board Id.
        /// </summary>
        public Guid BoardId { get; private set; }

        /// <summary>
        /// State is stored as JSON representing a list of live cell coordinates.
        /// </summary>
        public string State { get; private set; }

        /// <summary>
        /// State hash.
        /// </summary>
        public ulong StateHash { get; private set; }

        /// <summary>
        /// Iteration.
        /// </summary>
        public long Iteration { get; private set; }

        public State Status { get; set; }

        // For EF Core
        private BoardState() { }

        /// <summary>
        /// Creates a new BoardState from a list of live cell coordinates.
        /// </summary>
        /// <param name="initialState">The initial board as a list of CellCoordinates for live cells.</param>
        /// <param name="boardStateId">Optional Board Id.</param>
        /// <param name="previousBoard">The previous board state (if any), used to increment iteration and carry status.</param>
        public BoardState(List<CellCoordinates> initialState, Guid? boardStateId = null, BoardState? previousBoard = null)
        {
            if (initialState == null)
                throw new ArgumentNullException(nameof(initialState));

            Id = Guid.CreateVersion7();
            BoardId = boardStateId ?? Guid.CreateVersion7();

            // The input is already a sparse coordinate list.
            State = initialState.Serialize();
            StateHash = ComputeBoardHash(initialState);
            Iteration = previousBoard?.Iteration + 1 ?? 0;
            Status = previousBoard?.Status ?? Enums.State.NotFinished;
        }

        /// <summary>
        /// Returns the list of live cell coordinates deserialized from the JSON state.
        /// </summary>
        public List<CellCoordinates> LiveCells => CellCoordinatesSerializer.Deserialize(State) ?? [];

        /// <summary>
        /// Computes the board hash using the FNV-1a algorithm on the sorted list of live cell coordinates.
        /// </summary>
        private ulong ComputeBoardHash(List<CellCoordinates> liveCells)
        {
            // Sort the coordinates to ensure a consistent order.
            var sorted = liveCells.OrderBy(c => c.Row).ThenBy(c => c.Col);
            const ulong fnvOffsetBasis = 1469598103934665603UL;
            const ulong fnvPrime = 1099511628211UL;
            ulong hash = fnvOffsetBasis;

            foreach (var cell in sorted)
            {
                // Combine row and col into a single number.
                hash ^= (ulong)(cell.Row * 31 + cell.Col);
                hash *= fnvPrime;
            }
            return hash;
        }        
    }
}
