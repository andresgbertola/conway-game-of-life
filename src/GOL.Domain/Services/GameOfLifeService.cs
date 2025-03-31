using GOL.Domain.Entities;
using GOL.Domain.Interfaces;

namespace GOL.Domain.Services
{
    /// <summary>
    /// Provides the simulation logic for Game of life.
    /// </summary>
    public class GameOfLifeService : IGameOfLifeService
    {
        /// <summary>
        /// Computes the next generation given a list of live cell coordinates.
        /// The algorithm counts the live neighbors for each cell adjacent to a live cell,
        /// applies rules (neighbors counted excluding the cell itself),
        /// and returns the new list of live cell coordinates.
        /// </summary>
        /// <param name="currentLiveCells">A list of CellCoordinates representing live cells.</param>
        /// <returns></returns>
        public List<CellCoordinates> ComputeNextGeneration(List<CellCoordinates> currentLiveCells)
        {
            // Validate input: ensure the provided list is not null.
            if (currentLiveCells == null)
                throw new ArgumentNullException(nameof(currentLiveCells));

            // Create a HashSet from the current live cells for O(1) lookups.
            // This is used later to quickly determine if a candidate cell is currently alive.
            var liveCellsLookup = new HashSet<CellCoordinates>(currentLiveCells);

            // Define the 8 relative neighbor offsets (row, col).
            // These represent the eight adjacent positions around a cell.
            var neighborOffsets = new (int dRow, int dCol)[]
            {
                (-1, -1), // top-left
                (-1,  0), // top
                (-1,  1), // top-right
                ( 0, -1), // left
                ( 0,  1), // right
                ( 1, -1), // bottom-left
                ( 1,  0), // bottoms
                ( 1,  1)  // bottom-right
            };

            // Create a dictionary to count the number of live neighbors for each candidate cell.
            // Preallocate capacity: each live cell can contribute up to 8 neighbor counts.
            var neighborCounts = new Dictionary<CellCoordinates, int>(currentLiveCells.Count * 8);

            // Loop through each currently live cell.
            foreach (var cell in currentLiveCells)
            {
                // For each live cell, iterate over each of its 8 neighbor positions.
                foreach (var offset in neighborOffsets)
                {
                    // Compute the neighbor's coordinates by adding the offset.
                    var neighbor = new CellCoordinates(cell.Row + offset.dRow, cell.Col + offset.dCol);

                    // Increment the neighbor count for this cell.
                    // TryGetValue is used to avoid a second lookup.
                    if (neighborCounts.TryGetValue(neighbor, out int count))
                        // Means that is already a neigbor of somebody else so we are summing the count with 1 more.
                        neighborCounts[neighbor] = count + 1;
                    else
                        // No other has this cell as neigbor yet. We start it with 1 then as this is neigbor of cell.
                        neighborCounts[neighbor] = 1;
                }
            }

            // Prepare a list to hold the coordinates of live cells for the next generation.
            var nextLiveCells = new List<CellCoordinates>();

            /* RULES
             * 1. Any live cell with fewer than two live neighbours dies, as if by underpopulation.
               2. Any live cell with two or three live neighbours lives on to the next generation.
               3. Any live cell with more than three live neighbours dies, as if by overpopulation.
               4. Any dead cell with exactly three live neighbours becomes a live cell, as if by reproduction.
            */

            // Evaluate each candidate cell (any cell that was adjacent to a current live cell)
            // to decide based on rules:
            //  - A live cell survives if it has 2 or 3 live neighbors.
            //  - A dead cell becomes live if it has exactly 3 live neighbors.
            foreach (var neigbor in neighborCounts)
            {
                var candidateCell = neigbor.Key;
                int liveNeighborCount = neigbor.Value;

                // Check if the candidate cell is currently alive.
                if (liveCellsLookup.Contains(candidateCell))
                {
                    // If the cell is alive, it survives if it has 2 or 3 live neighbors.
                    if (liveNeighborCount == 2 || liveNeighborCount == 3)
                        nextLiveCells.Add(candidateCell);
                }
                else
                {
                    // If the cell is dead, it becomes live if it has exactly 3 live neighbors.
                    if (liveNeighborCount == 3)
                        nextLiveCells.Add(candidateCell);
                }
            }

            // Return the computed list of live cells for the next generation.
            return nextLiveCells;
        }
    }
}