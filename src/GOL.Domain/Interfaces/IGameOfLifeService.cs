using GOL.Domain.Entities;

namespace GOL.Domain.Interfaces
{
    /// <summary>
    /// Provides the simulation logic for Game of life.
    /// </summary>
    public interface IGameOfLifeService
    {
        /// <summary>
        /// Computes the next generation given a list of live cell coordinates.
        /// The algorithm counts the live neighbors for each cell adjacent to a live cell,
        /// applies rules (neighbors counted excluding the cell itself),
        /// and returns the new list of live cell coordinates.
        /// </summary>
        /// <param name="currentLiveCells">A list of CellCoordinates representing live cells.</param>
        /// <returns></returns>
        List<CellCoordinates> ComputeNextGeneration(List<CellCoordinates> currentLiveCells);
    }
}
